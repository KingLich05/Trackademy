using Microsoft.AspNetCore.Http;
using Trackademy.Application.Users.Models;

namespace Trackademy.Application.Users.Interfaces;

public interface IExcelImportService
{
    /// <summary>
    /// Парсит Excel файл и возвращает список строк для импорта
    /// </summary>
    Task<List<UserImportRow>> ParseExcelFile(IFormFile file);
}
