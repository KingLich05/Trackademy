using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Groups : Entity
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Level { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
    public ICollection<User> Students { get; set; } = new List<User>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}