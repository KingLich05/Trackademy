namespace Trackademy.Application.GroupServices.Models;

public class FreezeStudentRequest
{
    public Guid StudentId { get; set; }
    public Guid GroupId { get; set; }
    public DateOnly FrozenFrom { get; set; }
    public DateOnly FrozenTo { get; set; }
    public string? FreezeReason { get; set; }
}
