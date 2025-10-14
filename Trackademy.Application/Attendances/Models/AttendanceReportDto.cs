using Trackademy.Domain.Enums;

namespace Trackademy.Application.Attendances.Models;

public class AttendanceReportDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentLogin { get; set; } = string.Empty;
    public List<LessonAttendanceDto> Lessons { get; set; } = new();
}

public class LessonAttendanceDto
{
    public Guid LessonId { get; set; }
    public DateOnly Date { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
}