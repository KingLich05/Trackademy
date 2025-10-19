using Trackademy.Domain.Enums;
using Trackademy.Domain.Common;

namespace Trackademy.Domain.Users;

public class Submission : Entity
{
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public SubmissionStatus Status { get; set; }

    public User Student { get; set; } = null!;
    public Assignment Assignment { get; set; } = null!;
    public ICollection<Score> Scores { get; set; } = new List<Score>();
}