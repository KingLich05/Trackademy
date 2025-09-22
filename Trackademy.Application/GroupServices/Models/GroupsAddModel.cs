namespace Trackademy.Application.GroupServices.Models;

public class GroupsAddModel
{
    public string? Name { get; set; }
    
    public string? Code { get; set; }
    
    public string? Level { get; set; }

    public Guid SubjectId { get; set; }
    
    public List<Guid> StudentIds { get; set; }
    
    public Guid? OrganizationId { get; set; }
}