using Trackademy.Domain.Common;

namespace Trackademy.Domain.Users;

public class Assignment : Entity
{
    public string? Description { get; set; }
    public Guid GroupId { get; set; }

    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Groups Group { get; set; } = null!;
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}