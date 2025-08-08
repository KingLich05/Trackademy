using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Schedule : BaseEntity
{


    public int DayOfWeek { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
    
    public Guid GroupId { get; set; }
    
    public Guid SubjectId { get; set; }
    
    public Guid TeacherId { get; set; }
    
    public Guid RoomId { get; set; }

    #region нав поля

    public Groups Group { get; set; }
    
    public Subject Subject { get; set; }
    
    public User Teacher { get; set; }
    
    public Room Room { get; set; } = null!;
    
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    #endregion
}