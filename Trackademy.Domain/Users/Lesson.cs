using Trackademy.Domain.Enums;
using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Lesson: Entity
{
    public Guid ScheduleId { get; set; }
    public DateOnly Date   { get; set; }

    // фактические значения
    // то есть если schedule является фактом а lesson является фактом посещения или переноса
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime   { get; set; }
    public Guid SubjectId { get; set; }
    public Guid GroupId   { get; set; }
    public Guid TeacherId { get; set; }
    public Guid RoomId    { get; set; }

    public LessonStatus LessonStatus { get; set; } = LessonStatus.Planned;
    public string? CancelReason { get; set; }

    public string? Note     { get; set; }

    #region нав поля

    public Schedule Schedule { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Groups Group { get; set; } = null!;
    public User Teacher { get; set; } = null!;
    public Room Room { get; set; } = null!;

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Score> Scores { get; set; } = new List<Score>();

    #endregion
}