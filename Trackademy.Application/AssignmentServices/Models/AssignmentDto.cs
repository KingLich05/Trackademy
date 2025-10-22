using Trackademy.Application.Users.Models;

namespace Trackademy.Application.AssignmentServices.Models;

public class AssignmentDto
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public Guid GroupId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties для отображения
    public GroupMinimalViewModel? Group { get; set; }
}