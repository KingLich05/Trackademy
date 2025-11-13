using Trackademy.Application.Users.Models;

namespace Trackademy.Application.AssignmentServices.Models;

/// <summary>
/// Детальная информация о задании со списком студентов и их submissions
/// </summary>
public class AssignmentDetailedDto
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public Guid GroupId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public GroupMinimalViewModel? Group { get; set; }
    
    /// <summary>
    /// Список студентов группы с их submissions для этого задания
    /// </summary>
    public List<StudentSubmissionDto> StudentSubmissions { get; set; } = new();
}

/// <summary>
/// Информация о студенте и его submission для конкретного задания
/// </summary>
public class StudentSubmissionDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentLogin { get; set; }
    
    /// <summary>
    /// Submission студента (null если не сдавал)
    /// </summary>
    public StudentSubmissionInfo? Submission { get; set; }
}

/// <summary>
/// Краткая информация о submission студента
/// </summary>
public class StudentSubmissionInfo
{
    public Guid Id { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int? Score { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
}
