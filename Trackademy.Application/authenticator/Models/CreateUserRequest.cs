using Trackademy.Domain.Enums;

namespace Trackademy.Application.authenticator.Models;

public record CreateUserRequest
{
    public required string Login { get; set; }
    public string FullName { get; set; }
    
    public string? Password { get; set; }
    
    public string Phone { get; set; }

    public string? ParentPhone { get; set; }
    
    public DateOnly? Birthday { get; set; }

    public RoleEnum Role { get; set; }
    
    /// <summary>
    /// Флаг указывающий, что пользователь записан на пробный урок
    /// </summary>
    public bool IsTrial { get; set; } = false;
    
    public Guid OrganizationId { get; set; }
}