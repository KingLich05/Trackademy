namespace Trackademy.Application.Users.Models;

/// <summary>
/// Результат импорта пользователей из Excel
/// </summary>
public class UserImportResult
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<UserImportSuccess> CreatedUsers { get; set; } = new();
    public List<UserImportError> Errors { get; set; } = new();
}
