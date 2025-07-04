using Trackademy.Domain.Enums;

namespace Trackademy.Domain.Users;

public class Roles
{
    public Guid Id { get; set; }
    public RoleEnum Role { get; set; }
    
    public List<User> Users { get; set; }
}