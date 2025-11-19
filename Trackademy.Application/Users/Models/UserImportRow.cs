namespace Trackademy.Application.Users.Models;

/// <summary>
/// Модель для одной строки импорта пользователя из Excel
/// </summary>
public class UserImportRow
{
    public int RowNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ParentPhone { get; set; }
    public DateOnly? Birthday { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Login { get; set; }
}
