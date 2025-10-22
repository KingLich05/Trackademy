using Trackademy.Domain.Common;

namespace Trackademy.Domain.Users;

public class Subject : BaseEntity
{
    public string? Description { get; set; }
    
    public Guid OrganizationId { get; set; }
    
    // нав. поля.
    public List<Groups> Groups { get; set; }
    
    public Organization Organization { get; set; }
}