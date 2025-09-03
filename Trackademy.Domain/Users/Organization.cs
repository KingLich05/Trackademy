using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Organization : Entity
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    
    public List<User> Users { get; set; } = new List<User>();
}