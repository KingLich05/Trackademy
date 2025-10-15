using ClosedXML.Excel;
using Trackademy.Application.Attendances.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Services;

public class ExcelExportService : IExcelExportService
{
    public async Task<byte[]> ExportAttendanceReportAsync(List<AttendanceDto> attendances, AttendanceExportFilterModel filter)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Отчет по посещаемости");

            // Заголовок отчета
            worksheet.Cell(1, 1).Value = "ОТЧЕТ ПО ПОСЕЩАЕМОСТИ";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 8).Merge();

            // Информация о периоде
            worksheet.Cell(2, 1).Value = $"Период: {filter.FromDate:dd.MM.yyyy} - {filter.ToDate:dd.MM.yyyy}";
            worksheet.Cell(2, 1).Style.Font.Bold = true;
            worksheet.Range(2, 1, 2, 8).Merge();

            // Заголовки таблицы
            var headerRow = 4;
            var headers = new[]
            {
                "№", "Дата", "Студент", "Логин", "Группа", "Предмет", "Время", "Статус"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Данные
            var dataStartRow = headerRow + 1;
            for (int i = 0; i < attendances.Count; i++)
            {
                var attendance = attendances[i];
                var row = dataStartRow + i;

                worksheet.Cell(row, 1).Value = i + 1;
                worksheet.Cell(row, 2).Value = attendance.Date.ToString("dd.MM.yyyy");
                worksheet.Cell(row, 3).Value = attendance.StudentName;
                worksheet.Cell(row, 4).Value = attendance.StudentLogin;
                worksheet.Cell(row, 5).Value = attendance.GroupName;
                worksheet.Cell(row, 6).Value = attendance.SubjectName;
                worksheet.Cell(row, 7).Value = $"{attendance.StartTime:hh\\:mm} - {attendance.EndTime:hh\\:mm}";
                worksheet.Cell(row, 8).Value = attendance.StatusName;

                // Цветовое кодирование статуса
                var statusCell = worksheet.Cell(row, 8);
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

                // Границы для всех ячеек строки
                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
            }

            // Автоподбор ширины колонок
            worksheet.Columns().AdjustToContents();

            // Статистика
            if (attendances.Any())
            {
                var statsStartRow = dataStartRow + attendances.Count + 2;
                
                worksheet.Cell(statsStartRow, 1).Value = "СТАТИСТИКА:";
                worksheet.Cell(statsStartRow, 1).Style.Font.Bold = true;
                
                var totalLessons = attendances.Count;
                var attendedCount = attendances.Count(a => a.Status == AttendanceStatus.Attend);
                var missedCount = attendances.Count(a => a.Status == AttendanceStatus.NotAttend);
                var lateCount = attendances.Count(a => a.Status == AttendanceStatus.Late);
                var specialCount = attendances.Count(a => a.Status == AttendanceStatus.SpecialReason);
                
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

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        });
    }

    public async Task<byte[]> ExportGroupAttendanceReportAsync(List<AttendanceReportDto> groupReport, string groupName, DateOnly fromDate, DateOnly toDate)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"Отчет группы {groupName}");

            // Заголовок отчета
            worksheet.Cell(1, 1).Value = $"ОТЧЕТ ПО ПОСЕЩАЕМОСТИ ГРУППЫ: {groupName}";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;

            // Период
            worksheet.Cell(2, 1).Value = $"Период: {fromDate:dd.MM.yyyy} - {toDate:dd.MM.yyyy}";
            worksheet.Cell(2, 1).Style.Font.Bold = true;

            if (!groupReport.Any())
            {
                worksheet.Cell(4, 1).Value = "Нет данных за указанный период";
                using var emptyStream = new MemoryStream();
                workbook.SaveAs(emptyStream);
                return emptyStream.ToArray();
            }

            // Получаем все уникальные даты уроков
            var allLessons = groupReport
                .SelectMany(s => s.Lessons)
                .OrderBy(l => l.Date)
                .ThenBy(l => l.StartTime)
                .GroupBy(l => new { l.Date, l.SubjectName, l.StartTime })
                .Select(g => g.First())
                .ToList();

            // Заголовки: Студент + каждый урок
            var headerRow = 4;
            var col = 1;
            
            worksheet.Cell(headerRow, col++).Value = "Студент";
            worksheet.Cell(headerRow, col++).Value = "Логин";
            
            foreach (var lesson in allLessons)
            {
                var headerText = $"{lesson.Date:dd.MM}\n{lesson.SubjectName}\n{lesson.StartTime:hh\\:mm}";
                worksheet.Cell(headerRow, col).Value = headerText;
                worksheet.Cell(headerRow, col).Style.Alignment.WrapText = true;
                worksheet.Cell(headerRow, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                col++;
            }

            // Стиль заголовков
            var headerRange = worksheet.Range(headerRow, 1, headerRow, col - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Данные студентов
            var dataStartRow = headerRow + 1;
            for (int studentIndex = 0; studentIndex < groupReport.Count; studentIndex++)
            {
                var student = groupReport[studentIndex];
                var row = dataStartRow + studentIndex;
                col = 1;

                worksheet.Cell(row, col++).Value = student.StudentName;
                worksheet.Cell(row, col++).Value = student.StudentLogin;

                // Заполняем посещаемость для каждого урока
                foreach (var lesson in allLessons)
                {
                    var attendance = student.Lessons.FirstOrDefault(l => 
                        l.Date == lesson.Date && 
                        l.SubjectName == lesson.SubjectName && 
                        l.StartTime == lesson.StartTime);

                    var cell = worksheet.Cell(row, col);
                    
                    if (attendance != null)
                    {
                        // Используем сокращения для статусов
                        var statusCode = attendance.Status switch
                        {
                            AttendanceStatus.Attend => "П",
                            AttendanceStatus.NotAttend => "Н",
                            AttendanceStatus.Late => "О",
                            AttendanceStatus.SpecialReason => "У",
                            _ => "?"
                        };
                        
                        cell.Value = statusCode;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        
                        // Цветовое кодирование
                        switch (attendance.Status)
                        {
                            case AttendanceStatus.Attend:
                                cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                                break;
                            case AttendanceStatus.NotAttend:
                                cell.Style.Fill.BackgroundColor = XLColor.LightSalmon;
                                break;
                            case AttendanceStatus.Late:
                                cell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                                break;
                            case AttendanceStatus.SpecialReason:
                                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                                break;
                        }
                    }
                    else
                    {
                        cell.Value = "-";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                    
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    col++;
                }

                // Границы для строки студента
                for (int borderCol = 1; borderCol < col; borderCol++)
                {
                    worksheet.Cell(row, borderCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
            }

            // Автоподбор ширины колонок
            worksheet.Column(1).Width = 25; // Имя студента
            worksheet.Column(2).Width = 15; // Логин
            for (int lessonCol = 3; lessonCol < col; lessonCol++)
            {
                worksheet.Column(lessonCol).Width = 8; // Колонки уроков
            }

            // Легенда
            var legendStartRow = dataStartRow + groupReport.Count + 2;
            worksheet.Cell(legendStartRow, 1).Value = "ОБОЗНАЧЕНИЯ:";
            worksheet.Cell(legendStartRow, 1).Style.Font.Bold = true;
            
            worksheet.Cell(legendStartRow + 1, 1).Value = "П - Присутствовал";
            worksheet.Cell(legendStartRow + 1, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
            
            worksheet.Cell(legendStartRow + 2, 1).Value = "Н - Отсутствовал";
            worksheet.Cell(legendStartRow + 2, 1).Style.Fill.BackgroundColor = XLColor.LightSalmon;
            
            worksheet.Cell(legendStartRow + 3, 1).Value = "О - Опоздал";
            worksheet.Cell(legendStartRow + 3, 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
            
            worksheet.Cell(legendStartRow + 4, 1).Value = "У - Уважительная причина";
            worksheet.Cell(legendStartRow + 4, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            
            worksheet.Cell(legendStartRow + 5, 1).Value = "- - Урок не проводился";
            worksheet.Cell(legendStartRow + 5, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        });
    }
}