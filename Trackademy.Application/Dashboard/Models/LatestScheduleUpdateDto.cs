namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Информация о последнем обновлении расписания
/// </summary>
public class LatestScheduleUpdateDto
{
    public Guid ScheduleId { get; set; }
    
    /// <summary>
    /// Название группы
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// Предмет
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Преподаватель
    /// </summary>
    public string TeacherName { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата и время последнего обновления
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }
    
    /// <summary>
    /// Кто обновил расписание
    /// </summary>
    public string UpdatedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Тип изменения (создание, обновление, удаление)
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;
}