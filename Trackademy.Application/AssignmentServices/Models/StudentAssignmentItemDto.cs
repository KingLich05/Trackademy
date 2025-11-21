using Trackademy.Application.Users.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.AssignmentServices.Models;

/// <summary>
/// Задание для студента с минимальной информацией о сдаче
/// </summary>
public class StudentAssignmentItemDto
{
    public Guid AssignmentId { get; set; }
    public string? Description { get; set; }
    public Guid GroupId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
    
    public GroupMinimalViewModel? Group { get; set; }
    
    /// <summary>
    /// ID submission для получения деталей (null если не создан)
    /// </summary>
    public Guid? SubmissionId { get; set; }
    
    /// <summary>
    /// Статус submission (null если не создан)
    /// </summary>
    public SubmissionStatus? Status { get; set; }
    
    /// <summary>
    /// Оценка (null если не оценено)
    /// </summary>
    public int? Score { get; set; }
}
