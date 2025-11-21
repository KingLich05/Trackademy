namespace Trackademy.Application.GroupServices.Models;

public class UnfreezeStudentRequest
{
    public Guid StudentId { get; set; }
    public Guid GroupId { get; set; }
}
