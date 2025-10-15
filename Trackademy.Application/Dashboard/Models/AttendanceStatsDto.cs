namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Статистика по посещаемости
/// </summary>
public class AttendanceStatsDto
{
    /// <summary>
    /// Средняя посещаемость по всему центру (в процентах)
    /// </summary>
    public decimal OverallAttendanceRate { get; set; }
    
    /// <summary>
    /// Количество присутствующих студентов сегодня
    /// </summary>
    public int PresentStudentsToday { get; set; }
    
    /// <summary>
    /// Количество отсутствующих студентов сегодня
    /// </summary>
    public int AbsentStudentsToday { get; set; }
    
    /// <summary>
    /// Количество опоздавших студентов сегодня
    /// </summary>
    public int LateStudentsToday { get; set; }
    
    /// <summary>
    /// Посещаемость по группам
    /// </summary>
    public List<GroupAttendanceDto> GroupAttendanceRates { get; set; } = new();
}

/// <summary>
/// Посещаемость отдельной группы
/// </summary>
public class GroupAttendanceDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupCode { get; set; } = string.Empty;
    public decimal AttendanceRate { get; set; }
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
}