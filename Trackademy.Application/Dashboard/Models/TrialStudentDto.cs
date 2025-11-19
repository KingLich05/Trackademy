namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Студент, записанный на пробный урок
/// </summary>
public class TrialStudentDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>
    /// Предмет пробного урока
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата пробного урока
    /// </summary>
    public DateTime TrialLessonDate { get; set; }
    
    /// <summary>
    /// Время пробного урока
    /// </summary>
    public TimeSpan TrialLessonTime { get; set; }
    
    /// <summary>
    /// Преподаватель
    /// </summary>
    public string TeacherName { get; set; } = string.Empty;
    
    /// <summary>
    /// Статус пробного урока (запланирован, прошел, не явился)
    /// </summary>
    public string TrialStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата регистрации на пробный урок
    /// </summary>
    public DateTime RegisteredAt { get; set; }
}