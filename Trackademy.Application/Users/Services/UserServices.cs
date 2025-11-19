using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.authenticator.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.Exception;
using Trackademy.Application.Shared.Extensions;
using Trackademy.Application.Shared.Models;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.Services;

public class UserServices(TrackademyDbContext dbContext, IMapper mapper) :
    IUserServices
{
    public async Task<PagedResult<UserDto>> GetUsers(GetUserRequest getUserRequest)
    {
        var usersQuery = dbContext.Users
            .Where(x => x.Role != RoleEnum.Administrator 
                        && x.Role != RoleEnum.Owner
                        && x.OrganizationId == getUserRequest.OrganizationId)
            .Include(x => x.Groups)
            .AsQueryable();

        if (getUserRequest.search is { Length: > 0 } && !string.IsNullOrWhiteSpace(getUserRequest.search))
        {
            usersQuery = usersQuery.Where(x =>
                x.FullName.ToLower().Contains(getUserRequest.search.ToLower()) ||
                x.Login.ToLower().Contains(getUserRequest.search.ToLower())
            );
        }

        if (getUserRequest.RoleIds != null && getUserRequest.RoleIds.Count != 0)
        {
            usersQuery = usersQuery.Where(x => getUserRequest.RoleIds.Contains(x.Role));
        }

        if (getUserRequest.GroupIds != null && getUserRequest.GroupIds.Count != 0)
        {
            usersQuery = usersQuery.Where(x =>
                x.Groups.Any(g => getUserRequest.GroupIds.Contains(g.Id)));
        }

        var pagedUsers = await usersQuery
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .OrderBy(x => x.Name)
            .ToPagedResultAsync(getUserRequest);

        return pagedUsers;
    }

    public async Task<UserByIdDto> GetById(Guid id)
    {
        var user = await dbContext.Users
            .Include(x => x.Payments)
            .Include(x => x.Groups)
            .FirstOrDefaultAsync(x => x.Id == id);

        return mapper.Map<UserByIdDto>(user);
    }

    public async Task<UserCreationResult> CreateUser(CreateUserRequest request)
    {
        if (!ValidateData(request))
        {
            throw new ConflictException("Не все поля заполнены.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ConflictException("Пароль обязателен");
        }

        var organization = await dbContext.Organizations
            .Where(x => x.Id == request.OrganizationId)
            .FirstOrDefaultAsync();

        if (organization == null)
        {
            throw new ConflictException("Ошибка с организацией.");
        }

        var exists = await dbContext.Users
            .Where(x => x.OrganizationId == request.OrganizationId)
            .AnyAsync(u => u.Login == request.Login);

        if (exists)
        {
            throw new ConflictException("Пользователь с таким login уже существует.");
        }

        var user = new User
        {
            Login = request.Login,
            FullName = request.FullName,
            Phone = request.Phone,
            ParentPhone = request.ParentPhone,
            Role = request.Role,
            Birthday = request.Birthday,
            CreatedDate = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            OrganizationId = request.OrganizationId,
            Organization = organization,
            IsTrial = request.IsTrial
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return new UserCreationResult
        {
            IsSuccess = true,
            User = new UserCreatedDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Login = user.Login,
                Role = user.Role.ToString()
            }
        };
    }

    public async Task<Guid> UpdateUser(Guid id, UserUpdateModel request)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            throw new ConflictException("Пользователя с таким идентификатором не существует");
        }
        var exists = await dbContext.Users
            .AnyAsync(u => u.Login == request.Login && u.OrganizationId == user.OrganizationId && u.Id != id);

        if (exists)
        {
            throw new ConflictException("Такой логин уже существует в этой организации.");
        }

        user.Login = request.Login;
        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.ParentPhone = request.ParentPhone;
        user.Role = request.Role;
        user.Birthday = request.Birthday;
        user.IsTrial = request.IsTrial;

        await dbContext.SaveChangesAsync();
        return user.Id;
    }

    public async Task<bool> DeleteUser(Guid id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return false;
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePassword(UpdatePasswordRequest request)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.StudentId);

        if (user == null)
        {
            return false;
        }

        // Проверяем текущий пароль
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        // Хешируем новый пароль
        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        
        // Обновляем пароль
        user.PasswordHash = newPasswordHash;
        
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<UserImportResult> ImportUsersFromExcel(List<UserImportRow> rows, Guid organizationId)
    {
        var result = new UserImportResult
        {
            TotalRows = rows.Count
        };

        // Множество для отслеживания логинов, добавленных в текущей сессии импорта
        var usedLoginsInCurrentImport = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Загружаем все существующие логины в организации одним запросом
        var existingLogins = await dbContext.Users
            .Where(u => u.OrganizationId == organizationId)
            .Select(u => u.Login)
            .ToListAsync();
        var existingLoginsSet = new HashSet<string>(existingLogins, StringComparer.OrdinalIgnoreCase);
        
        // Список данных для создания пользователей
        var preparedData = new List<(string login, string fullName, string phone, string? parentPhone, 
            DateOnly birthday, string password, RoleEnum role, int rowNumber)>();

        // Первый проход: валидация и подготовка данных (без хеширования)
        foreach (var row in rows)
        {
            var errors = new List<string>();

            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(row.FullName))
                errors.Add("ФИО обязательно");

            if (string.IsNullOrWhiteSpace(row.Phone))
                errors.Add("Телефон обязателен");
            
            if (!row.Birthday.HasValue)
                errors.Add("Дата рождения обязательна");

            if (string.IsNullOrWhiteSpace(row.Role))
                errors.Add("Роль обязательна");

            // Валидация роли
            if (!string.IsNullOrWhiteSpace(row.Role) && 
                !Enum.TryParse<RoleEnum>(row.Role, true, out var roleEnum))
            {
                errors.Add($"Неверная роль. Допустимые значения: Student или Teacher");
            }

            // Если есть ошибки валидации - добавляем в список ошибок и пропускаем
            if (errors.Any())
            {
                result.Errors.Add(new UserImportError
                {
                    RowNumber = row.RowNumber,
                    FullName = row.FullName,
                    Phone = row.Phone ?? string.Empty,
                    ParentPhone = row.ParentPhone,
                    Birthday = row.Birthday?.ToString("dd.MM.yyyy"),
                    Role = row.Role,
                    Login = row.Login,
                    Errors = errors
                });
                result.ErrorCount++;
                continue;
            }

            try
            {
                // Генерируем логин если не указан
                var login = !string.IsNullOrWhiteSpace(row.Login) 
                    ? row.Login 
                    : GenerateLoginFromFullName(row.FullName);

                // Проверяем уникальность логина (проверяем в памяти, без запроса к БД)
                login = EnsureUniqueLoginInMemory(login, existingLoginsSet, usedLoginsInCurrentImport);
                
                // Добавляем логин в множество использованных в текущей сессии
                usedLoginsInCurrentImport.Add(login);
                existingLoginsSet.Add(login); // Добавляем и в существующие, чтобы избежать дублей

                // Форматируем телефон
                var formattedPhone = FormatPhone(row.Phone);

                // Генерируем пароль из даты рождения (ДДММГГ)
                var password = GeneratePasswordFromBirthday(row.Birthday!.Value);

                // Парсим роль
                var role = Enum.Parse<RoleEnum>(row.Role, true);

                // Добавляем подготовленные данные (без хеширования пароля)
                preparedData.Add((login, row.FullName, formattedPhone, row.ParentPhone, 
                    row.Birthday.Value, password, role, row.RowNumber));
            }
            catch (Exception ex)
            {
                result.Errors.Add(new UserImportError
                {
                    RowNumber = row.RowNumber,
                    FullName = row.FullName,
                    Phone = row.Phone ?? string.Empty,
                    ParentPhone = row.ParentPhone,
                    Birthday = row.Birthday?.ToString("dd.MM.yyyy"),
                    Role = row.Role,
                    Login = row.Login,
                    Errors = new List<string> { $"Ошибка при подготовке: {ex.Message}" }
                });
                result.ErrorCount++;
            }
        }

        // Второй проход: параллельное хеширование паролей
        var usersToCreate = new List<(User user, string password, int rowNumber)>();
        
        if (preparedData.Any())
        {
            // Хешируем пароли параллельно (это CPU-bound операция)
            var hashedPasswords = await Task.Run(() => 
                preparedData.AsParallel()
                    .AsOrdered()
                    .Select(data => BCrypt.Net.BCrypt.HashPassword(data.password))
                    .ToList()
            );

            // Создаем пользователей с уже захешированными паролями
            for (int i = 0; i < preparedData.Count; i++)
            {
                var data = preparedData[i];
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Login = data.login,
                    FullName = data.fullName,
                    Phone = data.phone,
                    ParentPhone = data.parentPhone,
                    Birthday = data.birthday,
                    PasswordHash = hashedPasswords[i],
                    Role = data.role,
                    OrganizationId = organizationId,
                    CreatedDate = DateTime.UtcNow,
                    IsTrial = false
                };

                usersToCreate.Add((user, data.password, data.rowNumber));
            }
        }

        // Второй проход: батч-вставка всех пользователей одним запросом
        if (usersToCreate.Any())
        {
            try
            {
                // Добавляем всех пользователей в контекст
                foreach (var (user, _, _) in usersToCreate)
                {
                    dbContext.Users.Add(user);
                }

                // Сохраняем все изменения одним запросом
                await dbContext.SaveChangesAsync();

                // Добавляем успешно созданных пользователей в результат
                foreach (var (user, password, rowNumber) in usersToCreate)
                {
                    result.CreatedUsers.Add(new UserImportSuccess
                    {
                        RowNumber = rowNumber,
                        UserId = user.Id,
                        FullName = user.FullName,
                        Login = user.Login,
                        GeneratedPassword = password,
                        Phone = user.Phone,
                        Role = user.Role.ToString()
                    });
                    result.SuccessCount++;
                }
            }
            catch (Exception ex)
            {
                // Если батч-вставка не удалась, добавляем все как ошибки
                foreach (var (user, _, rowNumber) in usersToCreate)
                {
                    result.Errors.Add(new UserImportError
                    {
                        RowNumber = rowNumber,
                        FullName = user.FullName,
                        Phone = user.Phone,
                        ParentPhone = user.ParentPhone,
                        Birthday = user.Birthday?.ToString("dd.MM.yyyy"),
                        Role = user.Role.ToString(),
                        Login = user.Login,
                        Errors = new List<string> { $"Ошибка при сохранении: {ex.Message}" }
                    });
                    result.ErrorCount++;
                }
            }
        }

        return result;
    }

    #region Private methods

    private bool ValidateData(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName)) return false;
        if (string.IsNullOrWhiteSpace(request.Phone)) return false;
        if (string.IsNullOrWhiteSpace(request.Password)) return false;
        if (!Enum.IsDefined(typeof(RoleEnum), request.Role)) return false;

        return true;
    }

    /// <summary>
    /// Генерирует логин из ФИО (транслитерация + lowercase)
    /// </summary>
    private string GenerateLoginFromFullName(string fullName)
    {
        // Словарь для транслитерации
        var translitMap = new Dictionary<char, string>
        {
            {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"}, {'е', "e"}, {'ё', "yo"},
            {'ж', "zh"}, {'з', "z"}, {'и', "i"}, {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"},
            {'н', "n"}, {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"}, {'у', "u"},
            {'ф', "f"}, {'х', "h"}, {'ц', "ts"}, {'ч', "ch"}, {'ш', "sh"}, {'щ', "sch"}, {'ъ', ""},
            {'ы', "y"}, {'ь', ""}, {'э', "e"}, {'ю', "yu"}, {'я', "ya"}
        };

        var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "user";

        var result = new System.Text.StringBuilder();
        
        // Берем первую букву имени и фамилию целиком
        if (parts.Length >= 2)
        {
            // Фамилия
            foreach (var ch in parts[0].ToLower())
            {
                if (translitMap.TryGetValue(ch, out var translitCh))
                    result.Append(translitCh);
                else if (char.IsLetterOrDigit(ch))
                    result.Append(ch);
            }
            result.Append('_');
            
            // Первая буква имени
            var firstNameChar = parts[1].ToLower()[0];
            if (translitMap.TryGetValue(firstNameChar, out var translitFirstName))
                result.Append(translitFirstName);
            else if (char.IsLetterOrDigit(firstNameChar))
                result.Append(firstNameChar);
        }
        else
        {
            // Только одно слово
            foreach (var ch in parts[0].ToLower())
            {
                if (translitMap.TryGetValue(ch, out var translitCh))
                    result.Append(translitCh);
                else if (char.IsLetterOrDigit(ch))
                    result.Append(ch);
            }
        }

        return result.Length > 0 ? result.ToString() : "user";
    }

    /// <summary>
    /// Проверяет уникальность логина и добавляет суффикс при дубликате
    /// </summary>
    private async Task<string> EnsureUniqueLogin(string baseLogin, Guid organizationId)
    {
        var login = baseLogin;
        var counter = 1;

        while (await dbContext.Users.AnyAsync(u => 
            u.Login == login && u.OrganizationId == organizationId))
        {
            counter++;
            login = $"{baseLogin}_{counter}";
        }

        return login;
    }

    /// <summary>
    /// Проверяет уникальность логина с учетом текущей сессии импорта и БД
    /// </summary>
    private async Task<string> EnsureUniqueLoginWithSession(
        string baseLogin, 
        Guid organizationId, 
        HashSet<string> usedLoginsInSession)
    {
        var login = baseLogin;
        var counter = 1;

        // Проверяем уникальность как в БД, так и в текущей сессии импорта
        while (await dbContext.Users.AnyAsync(u => 
                   u.Login == login && u.OrganizationId == organizationId) ||
               usedLoginsInSession.Contains(login))
        {
            counter++;
            login = $"{baseLogin}_{counter}";
        }

        return login;
    }

    /// <summary>
    /// Проверяет уникальность логина в памяти (без запросов к БД)
    /// </summary>
    private string EnsureUniqueLoginInMemory(
        string baseLogin, 
        HashSet<string> existingLogins,
        HashSet<string> usedLoginsInSession)
    {
        var login = baseLogin;
        var counter = 1;

        // Проверяем уникальность в обоих множествах (уже существующие и текущая сессия)
        while (existingLogins.Contains(login) || usedLoginsInSession.Contains(login))
        {
            counter++;
            login = $"{baseLogin}_{counter}";
        }

        return login;
    }

    /// <summary>
    /// Форматирует телефон в формат 8XXXXXXXXXX
    /// </summary>
    private string FormatPhone(string phone)
    {
        // Удаляем все символы кроме цифр
        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

        // Если начинается с +7, заменяем на 8
        if (digitsOnly.StartsWith("7") && digitsOnly.Length == 11)
        {
            digitsOnly = "8" + digitsOnly.Substring(1);
        }

        // Если начинается с 8 и длина 11 - возвращаем как есть
        if (digitsOnly.StartsWith("8") && digitsOnly.Length == 11)
        {
            return digitsOnly;
        }

        // Если длина 10 цифр - добавляем 8 в начало
        if (digitsOnly.Length == 10)
        {
            return "8" + digitsOnly;
        }

        // Возвращаем как есть если не удалось отформатировать
        return digitsOnly;
    }

    /// <summary>
    /// Генерирует пароль из даты рождения в формате ДДММГГ
    /// </summary>
    private string GeneratePasswordFromBirthday(DateOnly birthday)
    {
        var day = birthday.Day.ToString("D2");
        var month = birthday.Month.ToString("D2");
        var year = birthday.Year.ToString().Substring(2, 2); // Последние 2 цифры года

        return $"{day}{month}{year}";
    }

    #endregion

    /// <summary>
    /// Генерирует Excel шаблон для импорта пользователей
    /// </summary>
    public byte[] GenerateImportTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Шаблон");

        // Настройка заголовков
        worksheet.Cell(1, 1).Value = "ФИО";
        worksheet.Cell(1, 2).Value = "Телефон";
        worksheet.Cell(1, 3).Value = "Телефон родителя";
        worksheet.Cell(1, 4).Value = "Дата рождения";
        worksheet.Cell(1, 5).Value = "Роль";
        worksheet.Cell(1, 6).Value = "Логин";

        // Форматирование заголовков
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Автоширина колонок
        worksheet.Column(1).Width = 30; // ФИО
        worksheet.Column(2).Width = 15; // Телефон
        worksheet.Column(3).Width = 20; // Телефон родителя
        worksheet.Column(4).Width = 18; // Дата рождения
        worksheet.Column(6).Width = 12; // Роль
        worksheet.Column(7).Width = 15; // Логин

        // Добавление примера данных (опционально)
        worksheet.Cell(2, 1).Value = "Иванов Иван Иванович";
        worksheet.Cell(2, 2).Value = "ivanov@example.com";
        worksheet.Cell(2, 3).Value = "87001234567";
        worksheet.Cell(2, 4).Value = "87009876543";
        worksheet.Cell(2, 5).Value = "01.01.2005";
        worksheet.Cell(2, 6).Value = "Student";
        worksheet.Cell(2, 7).Value = ""; // Автогенерация

        // Форматирование примера (серый цвет для указания что это пример)
        var exampleRange = worksheet.Range(2, 1, 2, 7);
        exampleRange.Style.Font.Italic = true;
        exampleRange.Style.Font.FontColor = XLColor.Gray;

        // Добавление подсказок в соседних ячейках (строка 3)
        worksheet.Cell(3, 2).Value = "(необязательно)";
        worksheet.Cell(3, 4).Value = "(необязательно)";
        worksheet.Cell(3, 5).Value = "(формат: ДД.ММ.ГГГГ)";
        worksheet.Cell(3, 6).Value = "(Student или Teacher)";
        worksheet.Cell(3, 7).Value = "(автогенерация)";
        
        var hintRange = worksheet.Range(3, 1, 3, 7);
        hintRange.Style.Font.FontSize = 9;
        hintRange.Style.Font.FontColor = XLColor.DarkGray;
        hintRange.Style.Font.Italic = true;

        // Сохранение в поток
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}