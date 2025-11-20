namespace Trackademy.Application.SubjectServices.Models;

public class SubjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int? Price { get; set; }
}