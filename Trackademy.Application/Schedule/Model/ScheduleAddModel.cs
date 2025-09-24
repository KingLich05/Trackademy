namespace Trackademy.Application.Schedule.Model;

public class ScheduleAddModel
{
    public int[]? DaysOfWeek { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
    
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo  { get; set; }
    
    public Guid GroupId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid RoomId { get; set; }
    
    public Guid OrganizationId { get; set; }
}