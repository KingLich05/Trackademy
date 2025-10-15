namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Статистика по студентам
/// </summary>
public class StudentStatsDto
{
    /// <summary>
    /// Общее количество студентов
    /// </summary>
    public int TotalStudents { get; set; }
    
    /// <summary>
    /// Количество активных студентов (тех, кто был на занятиях в последние 30 дней)
    /// </summary>
    public int ActiveStudents { get; set; }
    
    /// <summary>
    /// Количество новых студентов за текущий месяц
    /// </summary>
    public int NewStudentsThisMonth { get; set; }
    
    /// <summary>
    /// Процент активности студентов
    /// </summary>
    public decimal ActivityPercentage => TotalStudents > 0 ? (decimal)ActiveStudents / TotalStudents * 100 : 0;
}