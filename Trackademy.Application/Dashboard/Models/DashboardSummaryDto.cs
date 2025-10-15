namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Краткая сводка дашборда - основные метрики
/// </summary>
public class DashboardSummaryDto
{
    /// <summary>
    /// Общее количество студентов
    /// </summary>
    public int TotalStudents { get; set; }
    
    /// <summary>
    /// Активные студенты
    /// </summary>
    public int ActiveStudents { get; set; }
    
    /// <summary>
    /// Общее количество групп
    /// </summary>
    public int TotalGroups { get; set; }
    
    /// <summary>
    /// Активные группы
    /// </summary>
    public int ActiveGroups { get; set; }
    
    /// <summary>
    /// Уроки на сегодня
    /// </summary>
    public int LessonsToday { get; set; }
    
    /// <summary>
    /// Завершенные уроки сегодня
    /// </summary>
    public int CompletedLessonsToday { get; set; }
    
    /// <summary>
    /// Средняя посещаемость по всему центру (%)
    /// </summary>
    public decimal AverageAttendanceRate { get; set; }
    
    /// <summary>
    /// Количество неоплативших студентов
    /// </summary>
    public int UnpaidStudentsCount { get; set; }
    
    /// <summary>
    /// Количество пробных студентов
    /// </summary>
    public int TrialStudentsCount { get; set; }
    
    /// <summary>
    /// Количество групп с низкой успеваемостью
    /// </summary>
    public int LowPerformanceGroupsCount { get; set; }
    
    /// <summary>
    /// Общая задолженность
    /// </summary>
    public decimal TotalDebt { get; set; }
    
    /// <summary>
    /// Дата последнего обновления данных
    /// </summary>
    public DateTime LastUpdated { get; set; }
}