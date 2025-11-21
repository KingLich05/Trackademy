namespace Trackademy.Application.AssignmentServices.Models;

/// <summary>
/// Ответ API с группировкой заданий для студента по статусам
/// </summary>
public class MyAssignmentsResponseDto
{
    /// <summary>
    /// Задания в работе (Draft, Returned) или еще не начатые
    /// </summary>
    public List<StudentAssignmentItemDto> Pending { get; set; } = new();
    
    /// <summary>
    /// Отправлено на проверку
    /// </summary>
    public List<StudentAssignmentItemDto> Submitted { get; set; } = new();
    
    /// <summary>
    /// Проверено и оценено
    /// </summary>
    public List<StudentAssignmentItemDto> Graded { get; set; } = new();
    
    /// <summary>
    /// Просрочено (дедлайн прошел, но не сдано)
    /// </summary>
    public List<StudentAssignmentItemDto> Overdue { get; set; } = new();
}
