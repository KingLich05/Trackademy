namespace Trackademy.Application.SubjectServices.Models;

public class SubjectAddModel
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid OrganizationId { get; set; }
}