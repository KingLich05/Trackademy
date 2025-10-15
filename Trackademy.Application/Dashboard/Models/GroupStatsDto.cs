namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Статистика по группам
/// </summary>
public class GroupStatsDto
{
    /// <summary>
    /// Общее количество групп
    /// </summary>
    public int TotalGroups { get; set; }
    
    /// <summary>
    /// Количество активных групп (у которых есть занятия в ближайшие 7 дней)
    /// </summary>
    public int ActiveGroups { get; set; }
    
    /// <summary>
    /// Средний размер группы
    /// </summary>
    public decimal AverageGroupSize { get; set; }
    
    /// <summary>
    /// Процент заполненности групп
    /// </summary>
    public decimal GroupFillPercentage => TotalGroups > 0 ? (decimal)ActiveGroups / TotalGroups * 100 : 0;
}