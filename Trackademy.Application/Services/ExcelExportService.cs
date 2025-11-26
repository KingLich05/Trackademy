using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Attendances.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Services.Models;
using Trackademy.Application.Shared.Exception;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Services;

public class ExcelExportService : IExcelExportService
{
    private readonly TrackademyDbContext _context;

    public ExcelExportService(TrackademyDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportAttendanceReportAsync(List<AttendanceDto> attendances, AttendanceExportFilterModel filter)
    {
        using var workbook = new XLWorkbook();

        if (filter.GroupId.HasValue)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == filter.GroupId.Value);
            var groupAttendances = attendances.Where(a => a.GroupId == filter.GroupId.Value).ToList();
            
            if (group != null && groupAttendances.Any())
            {
                CreateGroupWorksheet(workbook, group.Name, groupAttendances, filter);
            }
        }
        else
        {
            CreateSummaryWorksheet(workbook, attendances, filter);

            // Получаем уникальные GroupId из данных посещаемости
            var groupIds = attendances.Select(a => a.GroupId).Distinct().ToList();

            // Загружаем группы по найденным ID и сортируем по алфавиту
            var groupsWithData = await _context.Groups
                .Where(g => g.OrganizationId == filter.OrganizationId && 
                           groupIds.Contains(g.Id))
                .OrderBy(g => g.Name)
                .ToListAsync();

            foreach (var group in groupsWithData)
            {
                var groupAttendances = attendances.Where(a => a.GroupId == group.Id).ToList();
                if (groupAttendances.Any())
                {
                    CreateGroupWorksheet(workbook, group.Name, groupAttendances, filter);
                }
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void CreateSummaryWorksheet(XLWorkbook workbook, List<AttendanceDto> attendances, AttendanceExportFilterModel filter)
    {
        var worksheet = workbook.Worksheets.Add("Общая сводка");

        worksheet.Cell(1, 1).Value = "ОБЩАЯ СВОДКА ПО ПОСЕЩАЕМОСТИ";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        worksheet.Range(1, 1, 1, 6).Merge();

        worksheet.Cell(2, 1).Value = $"Период: {filter.FromDate:dd.MM.yyyy} - {filter.ToDate:dd.MM.yyyy}";
        worksheet.Cell(2, 1).Style.Font.Bold = true;
        worksheet.Range(2, 1, 2, 6).Merge();

        var headerRow = 4;
        var headers = new[] { "№", "Группа", "Всего уроков", "Присутствовали", "Отсутствовали", "% посещаемости" };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        var groupStats = attendances
            .GroupBy(a => new { a.GroupId, a.GroupName })
            .Select(g => new
            {
                GroupName = g.Key.GroupName,
                TotalLessons = g.Count(),
                AttendedCount = g.Count(a => a.Status == AttendanceStatus.Attend),
                MissedCount = g.Count(a => a.Status == AttendanceStatus.NotAttend || a.Status == AttendanceStatus.SpecialReason),
                AttendancePercentage = g.Count() > 0 ? Math.Round((double)g.Count(a => a.Status == AttendanceStatus.Attend) / g.Count() * 100, 2) : 0
            })
            .OrderBy(g => g.GroupName)
            .ToList();

        var dataStartRow = headerRow + 1;
        for (int i = 0; i < groupStats.Count; i++)
        {
            var stat = groupStats[i];
            var row = dataStartRow + i;

            worksheet.Cell(row, 1).Value = i + 1;
            worksheet.Cell(row, 2).Value = stat.GroupName;
            worksheet.Cell(row, 3).Value = stat.TotalLessons;
            worksheet.Cell(row, 4).Value = stat.AttendedCount;
            worksheet.Cell(row, 5).Value = stat.MissedCount;
            worksheet.Cell(row, 6).Value = $"{stat.AttendancePercentage}%";

            var percentageCell = worksheet.Cell(row, 6);
            if (stat.AttendancePercentage >= 90)
                percentageCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
            else if (stat.AttendancePercentage >= 75)
                percentageCell.Style.Fill.BackgroundColor = XLColor.LightYellow;
            else
                percentageCell.Style.Fill.BackgroundColor = XLColor.LightSalmon;

            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        worksheet.Columns().AdjustToContents();
    }

    private void CreateGroupWorksheet(XLWorkbook workbook, string groupName, List<AttendanceDto> groupAttendances, AttendanceExportFilterModel filter)
    {
        var worksheet = workbook.Worksheets.Add(SanitizeSheetName(groupName));

        worksheet.Cell(1, 1).Value = $"ОТЧЕТ ПО ПОСЕЩАЕМОСТИ - {groupName.ToUpper()}";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        worksheet.Range(1, 1, 1, 8).Merge();

        worksheet.Cell(2, 1).Value = $"Период: {filter.FromDate:dd.MM.yyyy} - {filter.ToDate:dd.MM.yyyy}";
        worksheet.Cell(2, 1).Style.Font.Bold = true;
        worksheet.Range(2, 1, 2, 8).Merge();

        var headerRow = 4;
        var headers = new[] { "№", "Дата", "Студент", "Логин", "Предмет", "Время", "Статус", "Примечание" };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        var sortedAttendances = groupAttendances.OrderBy(a => a.Date).ThenBy(a => a.StudentName).ToList();
        var dataStartRow = headerRow + 1;

        for (int i = 0; i < sortedAttendances.Count; i++)
        {
            var attendance = sortedAttendances[i];
            var row = dataStartRow + i;

            worksheet.Cell(row, 1).Value = i + 1;
            worksheet.Cell(row, 2).Value = attendance.Date.ToString("dd.MM.yyyy");
            worksheet.Cell(row, 3).Value = attendance.StudentName;
            worksheet.Cell(row, 4).Value = attendance.StudentLogin;
            worksheet.Cell(row, 5).Value = attendance.SubjectName;
            worksheet.Cell(row, 6).Value = $"{attendance.StartTime:hh\\:mm} - {attendance.EndTime:hh\\:mm}";
            worksheet.Cell(row, 7).Value = attendance.StatusName;
            worksheet.Cell(row, 8).Value = "";

            var statusCell = worksheet.Cell(row, 7);
            switch (attendance.Status)
            {
                case AttendanceStatus.Attend:
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    break;
                case AttendanceStatus.NotAttend:
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightSalmon;
                    break;
                case AttendanceStatus.Late:
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    break;
                case AttendanceStatus.SpecialReason:
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    break;
            }

            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        worksheet.Columns().AdjustToContents();

        if (sortedAttendances.Any())
        {
            var statsStartRow = dataStartRow + sortedAttendances.Count + 2;
            
            worksheet.Cell(statsStartRow, 1).Value = "СТАТИСТИКА ПО ГРУППЕ:";
            worksheet.Cell(statsStartRow, 1).Style.Font.Bold = true;
            
            var totalLessons = sortedAttendances.Count;
            var attendedCount = sortedAttendances.Count(a => a.Status == AttendanceStatus.Attend);
            var notAttendCount = sortedAttendances.Count(a => a.Status == AttendanceStatus.NotAttend);
            var specialReasonCount = sortedAttendances.Count(a => a.Status == AttendanceStatus.SpecialReason);
            var missedCount = notAttendCount + specialReasonCount; // Общее количество отсутствующих
            var lateCount = sortedAttendances.Count(a => a.Status == AttendanceStatus.Late);
            
            worksheet.Cell(statsStartRow + 1, 1).Value = $"Всего уроков: {totalLessons}";
            worksheet.Cell(statsStartRow + 2, 1).Value = $"Присутствовал: {attendedCount}";
            worksheet.Cell(statsStartRow + 3, 1).Value = $"Отсутствовал: {missedCount} (без причины: {notAttendCount}, по уважительной: {specialReasonCount})";
            worksheet.Cell(statsStartRow + 4, 1).Value = $"Опоздал: {lateCount}";
            
            if (totalLessons > 0)
            {
                var attendancePercentage = Math.Round((double)attendedCount / totalLessons * 100, 2);
                worksheet.Cell(statsStartRow + 5, 1).Value = $"Процент посещаемости: {attendancePercentage}%";
                worksheet.Cell(statsStartRow + 5, 1).Style.Font.Bold = true;
            }
        }
    }

    private string SanitizeSheetName(string name)
    {
        var invalidChars = new char[] { '\\', '/', '?', '*', '[', ']', ':' };
        foreach (var ch in invalidChars)
        {
            name = name.Replace(ch, '_');
        }
        
        if (name.Length > 31)
        {
            name = name.Substring(0, 28) + "...";
        }
        
        return name;
    }

    public async Task<byte[]> ExportUsersAsync(Guid organizationId)
    {
        // Проверка существования организации
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId);
        
        if (organization == null)
        {
            throw new ConflictException("Организация не найдена");
        }

        using var workbook = new XLWorkbook();

        // Экспорт студентов
        await CreateStudentsWorksheet(workbook, organizationId);

        // Экспорт преподавателей
        await CreateTeachersWorksheet(workbook, organizationId);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task CreateStudentsWorksheet(XLWorkbook workbook, Guid organizationId)
    {
        var worksheet = workbook.Worksheets.Add("Студенты");

        // Заголовки
        worksheet.Cell(1, 1).Value = "ФИО";
        worksheet.Cell(1, 2).Value = "Логин";
        worksheet.Cell(1, 3).Value = "Телефон";
        worksheet.Cell(1, 4).Value = "Группы";
        worksheet.Cell(1, 5).Value = "% Посещаемости";
        worksheet.Cell(1, 6).Value = "Средний балл";

        // Стиль заголовков
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Получение студентов
        var students = await _context.Users
            .Where(u => u.OrganizationId == organizationId && u.Role == RoleEnum.Student)
            .Include(u => u.Groups)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        // Предзагрузка всех данных одним запросом
        var studentIds = students.Select(s => s.Id).ToList();

        // Посещаемость: группируем по студентам
        var attendanceStats = await _context.Attendances
            .Where(a => studentIds.Contains(a.StudentId))
            .GroupBy(a => a.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                TotalCount = g.Count(),
                PresentCount = g.Count(a => a.Status == AttendanceStatus.Attend)
            })
            .ToListAsync();

        var attendanceDict = attendanceStats.ToDictionary(a => a.StudentId);

        // Средние баллы: группируем по студентам через Submission
        var scoreStats = await _context.Scores
            .Where(s => studentIds.Contains(s.Submission.StudentId) && s.NumericValue.HasValue)
            .GroupBy(s => s.Submission.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                AverageScore = g.Average(s => (double)s.NumericValue)
            })
            .ToListAsync();

        var scoreDict = scoreStats.ToDictionary(s => s.StudentId);

        int row = 2;
        foreach (var student in students)
        {
            // Список групп
            var groupNames = student.Groups.Select(g => g.Name).ToList();
            var groupsText = groupNames.Any() ? string.Join(", ", groupNames) : "Нет групп";

            // Посещаемость из предзагруженных данных
            var attendancePercent = 0.0;
            if (attendanceDict.TryGetValue(student.Id, out var attStats))
            {
                attendancePercent = attStats.TotalCount > 0
                    ? Math.Round((double)attStats.PresentCount / attStats.TotalCount * 100, 2)
                    : 0;
            }

            // Средний балл из предзагруженных данных
            var averageScore = 0.0;
            if (scoreDict.TryGetValue(student.Id, out var scoreAvg))
            {
                averageScore = Math.Round(scoreAvg.AverageScore, 2);
            }

            worksheet.Cell(row, 1).Value = student.FullName;
            worksheet.Cell(row, 2).Value = student.Login;
            worksheet.Cell(row, 3).Value = student.Phone;
            worksheet.Cell(row, 4).Value = groupsText;
            worksheet.Cell(row, 5).Value = $"{attendancePercent}%";
            worksheet.Cell(row, 6).Value = averageScore;

            row++;
        }

        // Автоширина колонок
        worksheet.Columns().AdjustToContents();
    }

    private async Task CreateTeachersWorksheet(XLWorkbook workbook, Guid organizationId)
    {
        var worksheet = workbook.Worksheets.Add("Преподаватели");

        // Заголовки
        worksheet.Cell(1, 1).Value = "ФИО";
        worksheet.Cell(1, 2).Value = "Логин";
        worksheet.Cell(1, 3).Value = "Телефон";
        worksheet.Cell(1, 4).Value = "Предметы";
        worksheet.Cell(1, 5).Value = "Количество групп";
        worksheet.Cell(1, 6).Value = "Количество уроков";

        // Стиль заголовков
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Получение преподавателей
        var teachers = await _context.Users
            .Where(u => u.OrganizationId == organizationId && u.Role == RoleEnum.Teacher)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        int row = 2;
        foreach (var teacher in teachers)
        {
            // Получение уникальных предметов через расписание
            var subjects = await _context.Schedules
                .Where(s => s.TeacherId == teacher.Id && s.OrganizationId == organizationId)
                .Include(s => s.Group)
                .ThenInclude(g => g.Subject)
                .Select(s => s.Group.Subject.Name)
                .Distinct()
                .ToListAsync();
            
            var subjectsText = subjects.Any() ? string.Join(", ", subjects) : "Нет предметов";

            // Количество групп
            var groupCount = await _context.Schedules
                .Where(s => s.TeacherId == teacher.Id && s.OrganizationId == organizationId)
                .Select(s => s.GroupId)
                .Distinct()
                .CountAsync();

            // Количество уроков
            var lessonsCount = await _context.Lessons
                .Where(l => l.TeacherId == teacher.Id)
                .CountAsync();

            worksheet.Cell(row, 1).Value = teacher.FullName;
            worksheet.Cell(row, 2).Value = teacher.Login;
            worksheet.Cell(row, 3).Value = teacher.Phone;
            worksheet.Cell(row, 4).Value = subjectsText;
            worksheet.Cell(row, 5).Value = groupCount;
            worksheet.Cell(row, 6).Value = lessonsCount;

            row++;
        }

        // Автоширина колонок
        worksheet.Columns().AdjustToContents();
    }

    public async Task<byte[]> ExportGroupsAsync(ExportGroupsRequest request)
    {
        // Проверка существования организации
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId);

        if (organization == null)
        {
            throw new ConflictException("Организация не найдена");
        }

        using var workbook = new XLWorkbook();

        // Получение групп
        var groupsQuery = _context.Groups
            .Where(g => g.OrganizationId == request.OrganizationId)
            .Include(g => g.Subject)
            .Include(g => g.Students)
            .AsQueryable();

        // Получение расписаний для групп (для преподавателей)
        var groupSchedules = await _context.Schedules
            .Where(s => s.OrganizationId == request.OrganizationId)
            .Include(s => s.Teacher)
            .ToListAsync();

        var groupTeacherDict = groupSchedules
            .GroupBy(s => s.GroupId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => s.Teacher.FullName).Distinct().ToList()
            );

        if (request.GroupId.HasValue)
        {
            groupsQuery = groupsQuery.Where(g => g.Id == request.GroupId.Value);
        }

        var groups = await groupsQuery
            .OrderBy(g => g.Name)
            .ToListAsync();

        if (!groups.Any())
        {
            throw new ConflictException("Группы не найдены");
        }

        // Получение всех студентов из групп
        var allStudentIds = groups.SelectMany(g => g.Students.Select(s => s.Id)).Distinct().ToList();
        var allGroupIds = groups.Select(g => g.Id).ToList();

        // Предзагрузка данных посещаемости по группам и студентам (через Lesson)
        var attendanceStats = await _context.Attendances
            .Where(a => allStudentIds.Contains(a.StudentId))
            .Include(a => a.Lesson)
            .Where(a => allGroupIds.Contains(a.Lesson.GroupId))
            .GroupBy(a => new { a.StudentId, a.Lesson.GroupId })
            .Select(g => new
            {
                g.Key.StudentId,
                g.Key.GroupId,
                TotalCount = g.Count(),
                PresentCount = g.Count(a => a.Status == AttendanceStatus.Attend)
            })
            .ToListAsync();

        var attendanceDict = attendanceStats.ToDictionary(
            a => (StudentId: a.StudentId, GroupId: a.GroupId),
            a => (TotalCount: a.TotalCount, PresentCount: a.PresentCount)
        );

        // Предзагрузка платежей (если требуется)
        Dictionary<Guid, PaymentInfo>? paymentDict = null;
        if (request.IncludePayments)
        {
            var payments = await _context.Payments
                .Where(p => allStudentIds.Contains(p.StudentId))
                .GroupBy(p => p.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    LastPayment = g.OrderByDescending(p => p.PaidAt ?? p.CreatedAt).FirstOrDefault()
                })
                .ToListAsync();

            paymentDict = payments
                .Where(p => p.LastPayment != null)
                .ToDictionary(
                    p => p.StudentId,
                    p => new PaymentInfo
                    {
                        Date = p.LastPayment!.PaidAt?.ToString("dd.MM.yyyy") ?? "-",
                        Amount = p.LastPayment.Amount,
                        Status = p.LastPayment.Status.ToString()
                    });
        }

        // Создание листа для каждой группы
        foreach (var group in groups)
        {
            var teachers = groupTeacherDict.TryGetValue(group.Id, out var teacherList) ? teacherList : new List<string>();
            CreateGroupStudentsWorksheet(workbook, group, teachers, attendanceDict, paymentDict, request.IncludePayments);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void CreateGroupStudentsWorksheet(
        XLWorkbook workbook,
        Domain.Users.Groups group,
        List<string> teachers,
        Dictionary<(Guid StudentId, Guid GroupId), (int TotalCount, int PresentCount)> attendanceDict,
        Dictionary<Guid, PaymentInfo>? paymentDict,
        bool includePayments)
    {
        var sheetName = SanitizeSheetName(group.Name);
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Шапка с информацией о группе
        worksheet.Cell(1, 1).Value = $"ГРУППА: {group.Name.ToUpper()}";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        var headerMergeColumns = includePayments ? 9 : 6;
        worksheet.Range(1, 1, 1, headerMergeColumns).Merge();

        worksheet.Cell(2, 1).Value = $"Предмет: {group.Subject.Name}";
        worksheet.Cell(2, 1).Style.Font.Bold = true;
        worksheet.Range(2, 1, 2, headerMergeColumns).Merge();

        worksheet.Cell(3, 1).Value = $"Код группы: {group.Code}";
        worksheet.Range(3, 1, 3, headerMergeColumns).Merge();

        // Преподаватель(и)
        if (teachers.Any())
        {
            var teacherNames = string.Join(", ", teachers);
            worksheet.Cell(4, 1).Value = $"Преподаватель: {teacherNames}";
            worksheet.Range(4, 1, 4, headerMergeColumns).Merge();
        }

        // Заголовки таблицы
        var headerRow = teachers.Any() ? 6 : 5;
        var col = 1;
        
        worksheet.Cell(headerRow, col++).Value = "№";
        worksheet.Cell(headerRow, col++).Value = "ФИО";
        worksheet.Cell(headerRow, col++).Value = "Логин";
        worksheet.Cell(headerRow, col++).Value = "Телефон";
        worksheet.Cell(headerRow, col++).Value = "Посещаемость";

        if (includePayments)
        {
            worksheet.Cell(headerRow, col++).Value = "Дата последнего платежа";
            worksheet.Cell(headerRow, col++).Value = "Сумма";
            worksheet.Cell(headerRow, col++).Value = "Статус платежа";
        }

        // Стиль заголовков
        var totalColumns = includePayments ? 8 : 5;
        var headerRange = worksheet.Range(headerRow, 1, headerRow, totalColumns);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Данные студентов
        var dataRow = headerRow + 1;
        var students = group.Students.OrderBy(s => s.FullName).ToList();
        var index = 1;

        foreach (var student in students)
        {
            col = 1;

            worksheet.Cell(dataRow, col++).Value = index++;
            worksheet.Cell(dataRow, col++).Value = student.FullName;
            worksheet.Cell(dataRow, col++).Value = student.Login;
            worksheet.Cell(dataRow, col++).Value = student.Phone ?? "-";

            // Посещаемость
            var attendanceText = "0/0 (0%)";
            if (attendanceDict.TryGetValue((student.Id, group.Id), out var attStats))
            {
                var attendancePercent = attStats.TotalCount > 0
                    ? Math.Round((double)attStats.PresentCount / attStats.TotalCount * 100, 2)
                    : 0;
                attendanceText = $"{attStats.PresentCount}/{attStats.TotalCount} ({attendancePercent}%)";
            }
            worksheet.Cell(dataRow, col++).Value = attendanceText;

            // Платежи (если требуется)
            if (includePayments)
            {
                if (paymentDict != null && paymentDict.TryGetValue(student.Id, out var payment))
                {
                    worksheet.Cell(dataRow, col++).Value = payment.Date;
                    worksheet.Cell(dataRow, col++).Value = payment.Amount;
                    worksheet.Cell(dataRow, col++).Value = payment.Status;
                }
                else
                {
                    worksheet.Cell(dataRow, col++).Value = "-";
                    worksheet.Cell(dataRow, col++).Value = "-";
                    worksheet.Cell(dataRow, col++).Value = "-";
                }
            }

            dataRow++;
        }

        // Автоширина колонок
        worksheet.Columns().AdjustToContents();
    }

    private class PaymentInfo
    {
        public string Date { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}