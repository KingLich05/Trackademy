using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Groups : Entity
{
    public string Name { get; set; }
    public string Code { get; set; }
    
    /// <summary>
    /// Описание группы если необоходимо это или все таки уровень это больше не описание а доп информация для группы
    /// </summary>
    public string? Level { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid OrganizationId { get; set; }

    #region Navigation

    public ICollection<User> Students { get; set; } = new List<User>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public Organization Organization { get; set; }

    #endregion
}