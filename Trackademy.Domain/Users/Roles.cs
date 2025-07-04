using Trackademy.Domain.Enums;
using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Roles : Entity
{
    public RoleEnum Role { get; set; }
    
    public List<User> Users { get; set; }
}