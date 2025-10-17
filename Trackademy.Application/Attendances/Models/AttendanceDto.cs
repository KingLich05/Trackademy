using Trackademy.Domain.Enums;

namespace Trackademy.Application.Attendances.Models;

public class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentLogin { get; set; } = string.Empty;
    public Guid LessonId { get; set; }
    public DateOnly Date { get; set; }
    public DateOnly PlannedLessonDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    
    public string SubjectName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}