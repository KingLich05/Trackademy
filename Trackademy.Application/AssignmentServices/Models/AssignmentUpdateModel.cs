namespace Trackademy.Application.AssignmentServices.Models;

public class AssignmentUpdateModel
{
    public string? Description { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
}