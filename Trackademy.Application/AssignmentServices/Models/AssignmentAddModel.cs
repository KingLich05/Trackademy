namespace Trackademy.Application.AssignmentServices.Models;

public class AssignmentAddModel
{
    public string? Description { get; set; }
    public Guid GroupId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
}