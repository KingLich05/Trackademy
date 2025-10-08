namespace Trackademy.Application.Lessons.Models;

public class LessonCustomAddModel
{
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public Guid GroupId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid RoomId { get; set; }

    public string? Note { get; set; }
}