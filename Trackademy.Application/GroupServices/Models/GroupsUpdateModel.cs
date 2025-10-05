namespace Trackademy.Application.GroupServices.Models;

public class GroupsUpdateModel
{
    public string? Name { get; set; }
    
    public string Code { get; set; }
    
    public string? Level { get; set; }

    public Guid SubjectId { get; set; }
    
    public List<Guid> StudentIds { get; set; }
}