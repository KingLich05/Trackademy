namespace Trackademy.Application.Users.Models;

/// <summary>
/// Модель успешно импортированного пользователя
/// </summary>
public class UserImportSuccess
{
    public int RowNumber { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string GeneratedPassword { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
