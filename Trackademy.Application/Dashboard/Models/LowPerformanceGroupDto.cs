namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Группа с низкой успеваемостью
/// </summary>
public class LowPerformanceGroupDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Процент посещаемости группы
    /// </summary>
    public decimal AttendanceRate { get; set; }
    
    /// <summary>
    /// Количество студентов в группе
    /// </summary>
    public int TotalStudents { get; set; }
    
    /// <summary>
    /// Количество активных студентов
    /// </summary>
    public int ActiveStudents { get; set; }
    
    /// <summary>
    /// Причина низкой успеваемости
    /// </summary>
    public string PerformanceIssue { get; set; } = string.Empty;
}