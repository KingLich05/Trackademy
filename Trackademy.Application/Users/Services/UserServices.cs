using AutoMapper;
using AutoMapper.QueryableExtensions;
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

        // Пароль обязателен для создания через API (email необязателен)
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
            Email = request.Email,
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
                Email = user.Email,
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
            .AnyAsync(u => u.Login == request.Login && u.Id != id);

        if (exists)
        {
            throw new ConflictException("Такой логин уже существует.");
        }

        user.Login = request.Login;
        user.FullName = request.FullName;
        user.Email = request.Email;
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
                errors.Add($"Неверная роль. Допустимые значения: {string.Join(", ", Enum.GetNames<RoleEnum>())}");
            }

            // Если есть ошибки валидации - добавляем в список ошибок и пропускаем
            if (errors.Any())
            {
                result.Errors.Add(new UserImportError
                {
                    RowNumber = row.RowNumber,
                    FullName = row.FullName,
                    Email = row.Email,
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

                // Проверяем уникальность логина и генерируем новый при дубликате
                login = await EnsureUniqueLogin(login, organizationId);

                // Форматируем телефон
                var formattedPhone = FormatPhone(row.Phone);

                // Генерируем пароль из даты рождения (ДДММГГ)
                var password = GeneratePasswordFromBirthday(row.Birthday!.Value);
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                // Парсим роль
                var role = Enum.Parse<RoleEnum>(row.Role, true);

                // Создаем пользователя
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Login = login,
                    FullName = row.FullName,
                    Email = row.Email,
                    Phone = formattedPhone,
                    ParentPhone = row.ParentPhone,
                    Birthday = row.Birthday,
                    PasswordHash = passwordHash,
                    Role = role,
                    OrganizationId = organizationId,
                    CreatedDate = DateTime.UtcNow,
                    IsTrial = false
                };

                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                result.CreatedUsers.Add(new UserImportSuccess
                {
                    RowNumber = row.RowNumber,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Login = user.Login,
                    GeneratedPassword = password,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role.ToString()
                });
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new UserImportError
                {
                    RowNumber = row.RowNumber,
                    FullName = row.FullName,
                    Email = row.Email,
                    Phone = row.Phone ?? string.Empty,
                    ParentPhone = row.ParentPhone,
                    Birthday = row.Birthday?.ToString("dd.MM.yyyy"),
                    Role = row.Role,
                    Login = row.Login,
                    Errors = new List<string> { $"Ошибка при создании: {ex.Message}" }
                });
                result.ErrorCount++;
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
}