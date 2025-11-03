namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Фильтры для дашборда
/// </summary>
public class DashboardFilterDto
{
    /// <summary>
    /// ID организации для фильтрации
    /// </summary>
    public Guid OrganizationId { get; set; }
    
    /// <summary>
    /// Конкретные группы для анализа
    /// </summary>
    public List<Guid>? GroupIds { get; set; }
    
    /// <summary>
    /// Конкретные предметы для анализа
    /// </summary>
    public List<Guid>? SubjectIds { get; set; }
    
    /// <summary>
    /// Включать ли неактивных студентов
    /// </summary>
    public bool IncludeInactiveStudents { get; set; } = false;
    
    /// <summary>
    /// Минимальный процент посещаемости для "низкой успеваемости"
    /// </summary>
    public decimal LowPerformanceThreshold { get; set; } = 70;
}