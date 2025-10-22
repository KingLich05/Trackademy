using Trackademy.Domain.Enums;
using Trackademy.Domain.Common;

namespace Trackademy.Domain.Users;

public class Submission : Entity
{
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Текстовое содержимое домашнего задания
    /// </summary>
    public string? TextContent { get; set; }
    
    /// <summary>
    /// Дата создания (когда студент начал работать)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Дата отправки на проверку (null если еще черновик)
    /// </summary>
    public DateTime? SubmittedAt { get; set; }
    
    /// <summary>
    /// Дата проверки преподавателем (null если не проверено)
    /// </summary>
    public DateTime? GradedAt { get; set; }
    
    /// <summary>
    /// Комментарий преподавателя при проверке
    /// </summary>
    public string? TeacherComment { get; set; }
    
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;

    // Navigation properties
    public User Student { get; set; } = null!;
    public Assignment Assignment { get; set; } = null!;
    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<SubmissionFile> Files { get; set; } = new List<SubmissionFile>();
}