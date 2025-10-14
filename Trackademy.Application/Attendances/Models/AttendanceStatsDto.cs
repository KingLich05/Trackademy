namespace Trackademy.Application.Attendances.Models;

public class AttendanceStatsDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int AttendedLessons { get; set; }
    public int MissedLessons { get; set; }
    public int LateLessons { get; set; }
    public int SpecialReasonLessons { get; set; }
    public double AttendancePercentage { get; set; }
}