namespace Trackademy.Application.Users.Models;

/// <summary>
/// Информация об ошибке импорта для конкретной строки
/// </summary>
public class UserImportError
{
    public int RowNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public List<string> Errors { get; set; } = new();
}
