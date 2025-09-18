namespace Trackademy.Application.GroupServices.Models;

public class GroupsAddModel
{
    public string Name { get; set; }
    
    public string Code { get; set; }
    
    public string? Level { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }
    
    public Guid OrganizationId { get; set; }
}