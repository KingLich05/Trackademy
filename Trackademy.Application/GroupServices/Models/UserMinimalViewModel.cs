namespace Trackademy.Application.GroupServices.Models;

public class UserMinimalViewModel
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public bool IsFrozen { get; set; }
    public DateOnly? FrozenFrom { get; set; }
    public DateOnly? FrozenTo { get; set; }
    public string? FreezeReason { get; set; }
}