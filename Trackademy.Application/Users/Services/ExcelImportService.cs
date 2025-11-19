using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;

namespace Trackademy.Application.Users.Services;

public class ExcelImportService : IExcelImportService
{
    public async Task<List<UserImportRow>> ParseExcelFile(IFormFile file)
    {
        var users = new List<UserImportRow>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        // Находим первую строку с данными (пропускаем заголовки)
        var firstRowUsed = worksheet.FirstRowUsed().RowNumber() + 1;
        var lastRowUsed = worksheet.LastRowUsed()?.RowNumber() ?? firstRowUsed;

        for (int row = firstRowUsed; row <= lastRowUsed; row++)
        {
            var currentRow = worksheet.Row(row);
            
            // Пропускаем пустые строки
            if (currentRow.IsEmpty())
                continue;

            var userRow = new UserImportRow
            {
                RowNumber = row,
                FullName = GetCellValue(worksheet, row, 1), // Колонка A
                Phone = GetCellValue(worksheet, row, 2), // Колонка B
                ParentPhone = GetCellValueOrNull(worksheet, row, 3), // Колонка C
                Birthday = ParseDateOnly(GetCellValue(worksheet, row, 4)), // Колонка D
                Role = GetCellValue(worksheet, row, 5), // Колонка E
                Login = GetCellValueOrNull(worksheet, row, 6) // Колонка F (необязательно)
            };

            users.Add(userRow);
        }

        return users;
    }

    private string GetCellValue(IXLWorksheet worksheet, int row, int col)
    {
        return worksheet.Cell(row, col).GetValue<string>().Trim();
    }

    private string? GetCellValueOrNull(IXLWorksheet worksheet, int row, int col)
    {
        var value = worksheet.Cell(row, col).GetValue<string>().Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private DateOnly? ParseDateOnly(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Попытка распарсить разные форматы даты
        if (DateTime.TryParse(value, out var dateTime))
        {
            return DateOnly.FromDateTime(dateTime);
        }

        // Попытка распарсить формат dd.MM.yyyy
        if (DateOnly.TryParseExact(value, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var dateOnly))
        {
            return dateOnly;
        }

        return null;
    }
}
