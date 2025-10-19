using Trackademy.Domain.Common;

namespace Trackademy.Domain.Users;

public class Assignment : BaseEntity
{
    public string? Description { get; set; }
    public Guid SubjectId { get; set; }

    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }

    public Subject Subject { get; set; } = null!;
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}