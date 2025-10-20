namespace Trackademy.Application.Lessons.Models;

public class LessonRescheduleModel
{
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Note { get; set; }
}