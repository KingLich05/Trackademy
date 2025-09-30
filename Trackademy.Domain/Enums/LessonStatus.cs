namespace Trackademy.Domain.Enums;

public enum LessonStatus
{
    /// <summary>
    /// Запланирован как в самом начале.
    /// </summary>
    Planned = 1,
    
    /// <summary>
    /// Проведен урок.
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// Отменен урок.
    /// </summary>
    Cancelled = 3,
    
    /// <summary>
    /// хз нужен ли такой статус но если он перенес
    /// и сразу будет поле по какой причине пропущенно.
    /// </summary>
    Moved = 4
}