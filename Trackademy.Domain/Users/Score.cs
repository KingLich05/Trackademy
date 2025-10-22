using Trackademy.Domain.Common;
using Trackademy.Domain.Enums;

namespace Trackademy.Domain.Users;

public class Score : Entity
{
    /// <summary>
    /// ID работы студента
    /// </summary>
    public Guid SubmissionId { get; set; }
    
    /// <summary>
    /// ID преподавателя, который поставил оценку
    /// </summary>
    public Guid TeacherId { get; set; }
    
    /// <summary>
    /// Числовая оценка (0-100)
    /// </summary>
    public int? NumericValue { get; set; }
    
    /// <summary>
    /// Максимальное количество баллов для этого задания
    /// </summary>
    public int MaxPoints { get; set; } = 100;
    
    /// <summary>
    /// Комментарий преподавателя
    /// </summary>
    public string? Feedback { get; set; }
    
    /// <summary>
    /// Статус оценки
    /// </summary>
    public ScoreStatus Status { get; set; } = ScoreStatus.Final;
    
    /// <summary>
    /// Когда была выставлена оценка
    /// </summary>
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Когда оценка была обновлена
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Версия оценки (для истории изменений)
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// ID предыдущей версии оценки (если была изменена)
    /// </summary>
    public Guid? PreviousVersionId { get; set; }
    
    // Navigation properties
    public Submission Submission { get; set; } = null!;
    public User Teacher { get; set; } = null!;
    public Score? PreviousVersion { get; set; }
    public ICollection<Score> NextVersions { get; set; } = new List<Score>();
    
    /// <summary>
    /// Получить оценку в процентах (0-100)
    /// </summary>
    public double GetPercentageScore()
    {
        return (NumericValue ?? 0) * 100.0 / MaxPoints;
    }
    
    /// <summary>
    /// Получить текстовое представление оценки
    /// </summary>
    public string GetDisplayValue()
    {
        return $"{NumericValue ?? 0}/{MaxPoints}";
    }
}