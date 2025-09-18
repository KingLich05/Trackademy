using Trackademy.Domain.Enums;
using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Attendance : Entity
{
    public Guid StudentId { get; set; }
    public Guid LessonId  { get; set; }
    public DateOnly Date  { get; set; }
    public AttendanceStatus Status { get; set; }
    
    public User Student { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}