using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Score : Entity
{
    public Guid SubmissionId { get; set; }
    public int Points { get; set; }
    public string? Feedback { get; set; }
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    
    public Submission Submission { get; set; } = null!;
}