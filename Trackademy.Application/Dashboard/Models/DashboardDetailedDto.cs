namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Детальный отчет дашборда - расширенная информация
/// </summary>
public class DashboardDetailedDto
{
    /// <summary>
    /// Детальная статистика по студентам
    /// </summary>
    public StudentStatsDto StudentStats { get; set; } = new();
    
    /// <summary>
    /// Детальная статистика по группам
    /// </summary>
    public GroupStatsDto GroupStats { get; set; } = new();
    
    /// <summary>
    /// Детальная статистика по урокам
    /// </summary>
    public LessonStatsDto LessonStats { get; set; } = new();
    
    /// <summary>
    /// Детальная статистика по посещаемости
    /// </summary>
    public AttendanceStatsDto AttendanceStats { get; set; } = new();
    
    /// <summary>
    /// Группы с низкой успеваемостью (топ 10)
    /// </summary>
    public List<LowPerformanceGroupDto> LowPerformanceGroups { get; set; } = new();
    
    /// <summary>
    /// Неоплатившие студенты (топ 10 с наибольшим долгом)
    /// </summary>
    public List<UnpaidStudentDto> UnpaidStudents { get; set; } = new();
    
    /// <summary>
    /// Пробные студенты (топ 10 последних)
    /// </summary>
    public List<TrialStudentDto> TrialStudents { get; set; } = new();
    
    /// <summary>
    /// Топ преподаватели (топ 5)
    /// </summary>
    public List<TopTeacherDto> TopTeachers { get; set; } = new();
    
    /// <summary>
    /// Последнее обновление расписания
    /// </summary>
    public LatestScheduleUpdateDto? LatestScheduleUpdate { get; set; }
    
    /// <summary>
    /// Посещаемость по группам (топ 10)
    /// </summary>
    public List<GroupAttendanceDto> GroupAttendanceRates { get; set; } = new();
    
    /// <summary>
    /// Дата генерации отчета
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    
    /// <summary>
    /// Период, за который собрана статистика
    /// </summary>
    public string ReportPeriod { get; set; } = string.Empty;
}