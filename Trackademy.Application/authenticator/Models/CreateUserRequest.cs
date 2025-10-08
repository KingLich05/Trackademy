using Trackademy.Domain.Enums;

namespace Trackademy.Application.authenticator.Models;

public record CreateUserRequest
{
    public required string Login { get; set; }
    public string FullName { get; set; }
    
    public string Email { get; set; }
    
    public string? Password { get; set; }
    
    public string Phone { get; set; }

    public string? ParentPhone { get; set; }
    
    public DateTime? Birthday { get; set; }

    public RoleEnum Role { get; set; }
    
    public Guid OrganizationId { get; set; }
}