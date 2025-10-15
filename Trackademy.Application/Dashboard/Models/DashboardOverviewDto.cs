namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Основная модель дашборда со всей статистикой
/// </summary>
public class DashboardOverviewDto
{
    public StudentStatsDto StudentStats { get; set; } = new();
    public GroupStatsDto GroupStats { get; set; } = new();
    public LessonStatsDto LessonStats { get; set; } = new();
    public AttendanceStatsDto AttendanceStats { get; set; } = new();
    public List<LowPerformanceGroupDto> LowPerformanceGroups { get; set; } = new();
    public List<UnpaidStudentDto> UnpaidStudents { get; set; } = new();
    public List<TrialStudentDto> TrialStudents { get; set; } = new();
    public List<TopTeacherDto> TopTeachers { get; set; } = new();
    public LatestScheduleUpdateDto? LatestScheduleUpdate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}