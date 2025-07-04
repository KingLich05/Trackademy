using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class User : Entity
{

    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public string PasswordHash { get; set; }
    
    public string? PhotoPath { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public Roles Role { get; set; }

    public Guid RoleId { get; set; }
}