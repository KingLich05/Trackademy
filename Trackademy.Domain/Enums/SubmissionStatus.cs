namespace Trackademy.Domain.Enums;

public enum SubmissionStatus
{
    /// <summary>
    /// Черновик - студент начал работать, но не отправил
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// Отправлено на проверку
    /// </summary>
    Submitted = 1,
    
    /// <summary>
    /// Проверено и оценено преподавателем  
    /// </summary>
    Graded = 2,
    
    /// <summary>
    /// Возвращено на доработку
    /// </summary>
    Returned = 3,
    
    /// <summary>
    /// Просрочено (дедлайн прошел)
    /// </summary>
    Overdue = 4
}