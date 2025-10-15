namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Статистика по урокам
/// </summary>
public class LessonStatsDto
{
    /// <summary>
    /// Количество уроков на сегодня
    /// </summary>
    public int LessonsToday { get; set; }
    
    /// <summary>
    /// Количество завершенных уроков сегодня
    /// </summary>
    public int CompletedLessonsToday { get; set; }
    
    /// <summary>
    /// Количество отмененных уроков сегодня
    /// </summary>
    public int CancelledLessonsToday { get; set; }
    
    /// <summary>
    /// Общее количество уроков за текущий месяц
    /// </summary>
    public int LessonsThisMonth { get; set; }
    
    /// <summary>
    /// Процент завершенных уроков сегодня
    /// </summary>
    public decimal CompletionRateToday => LessonsToday > 0 ? (decimal)CompletedLessonsToday / LessonsToday * 100 : 0;
}