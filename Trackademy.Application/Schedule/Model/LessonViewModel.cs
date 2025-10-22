using Trackademy.Application.RoomServices.Models;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Application.Users.Models;

namespace Trackademy.Application.Schedule.Model;

public class LessonViewModel
{
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string LessonStatus { get; set; }

    public SubjectMinimalViewModel Subject { get; set; }

    public GroupMinimalViewModel Group { get; set; }

    public UserMinimalModel Teacher { get; set; }

    public RoomMinimalViewModel Room { get; set; }
    
    public List<StudentMinimalViewModel> Students { get; set; } = new();
    
    public string? CancelReason { get; set; }
    
    public string? Note { get; set; }
}