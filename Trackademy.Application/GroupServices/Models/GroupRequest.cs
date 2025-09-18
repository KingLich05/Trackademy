namespace Trackademy.Application.GroupServices.Models;

public class GroupRequest
{
    public List<Guid>? Ids { get; set; }
    
    public Guid OrganizationId { get; set; }
}