using Trackademy.Application.RoomServices.Models;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Application.Users.Models;

namespace Trackademy.Application.Schedule.Model;

public class ScheduleViewModel
{
    public Guid Id { get; set; }

    public int[]? DaysOfWeek { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
    
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo  { get; set; }
    
    public SubjectMinimalViewModel Subject { get; set; }

    public GroupMinimalViewModel Group { get; set; }

    public UserMinimalModel Teacher { get; set; }
    public RoomMinimalViewModel Room { get; set; }
}