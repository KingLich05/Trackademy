using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Attendances.Models;
using Trackademy.Application.Persistance;
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

            // Загружаем группы по найденным ID
            var groupsWithData = await _context.Groups
                .Where(g => g.OrganizationId == filter.OrganizationId && 
                           groupIds.Contains(g.Id))
                .OrderBy(g => g.CreatedAt)
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
                MissedCount = g.Count(a => a.Status == AttendanceStatus.NotAttend),
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
            var missedCount = sortedAttendances.Count(a => a.Status == AttendanceStatus.NotAttend);
            var lateCount = sortedAttendances.Count(a => a.Status == AttendanceStatus.Late);
            var specialCount = sortedAttendances.Count(a => a.Status == AttendanceStatus.SpecialReason);
            
            worksheet.Cell(statsStartRow + 1, 1).Value = $"Всего уроков: {totalLessons}";
            worksheet.Cell(statsStartRow + 2, 1).Value = $"Присутствовал: {attendedCount}";
            worksheet.Cell(statsStartRow + 3, 1).Value = $"Отсутствовал: {missedCount}";
            worksheet.Cell(statsStartRow + 4, 1).Value = $"Опоздал: {lateCount}";
            worksheet.Cell(statsStartRow + 5, 1).Value = $"Уважительная причина: {specialCount}";
            
            if (totalLessons > 0)
            {
                var attendancePercentage = Math.Round((double)attendedCount / totalLessons * 100, 2);
                worksheet.Cell(statsStartRow + 6, 1).Value = $"Процент посещаемости: {attendancePercentage}%";
                worksheet.Cell(statsStartRow + 6, 1).Style.Font.Bold = true;
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
}