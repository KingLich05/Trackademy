namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Статистика посещаемости группы
/// </summary>
public class GroupAttendanceDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Общее количество студентов в группе
    /// </summary>
    public int TotalStudents { get; set; }
    
    /// <summary>
    /// Средняя посещаемость группы (%)
    /// </summary>
    public decimal AverageAttendanceRate { get; set; }
    
    /// <summary>
    /// Присутствующие сегодня
    /// </summary>
    public int PresentToday { get; set; }
    
    /// <summary>
    /// Отсутствующие сегодня
    /// </summary>
    public int AbsentToday { get; set; }
    
    /// <summary>
    /// Опоздавшие сегодня
    /// </summary>
    public int LateToday { get; set; }
    
    /// <summary>
    /// Общее количество проведенных уроков
    /// </summary>
    public int TotalLessons { get; set; }
    
    /// <summary>
    /// Процент посещаемости за последнюю неделю
    /// </summary>
    public decimal WeeklyAttendanceRate { get; set; }
    
    /// <summary>
    /// Процент посещаемости за последний месяц
    /// </summary>
    public decimal MonthlyAttendanceRate { get; set; }
}