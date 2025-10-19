using Trackademy.Domain.Common;

namespace Trackademy.Domain.Users;

public class Organization : Entity
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    
    public List<User> Users { get; set; } = new List<User>();
    
    public List<Room> Rooms { get; set; } = new();

    public List<Subject> Subjects { get; set; } = new();
    
    public List<Groups> Groups { get; set; } = new();
}