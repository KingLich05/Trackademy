using Trackademy.Domain.hz;

namespace Trackademy.Domain.Users;

public class Subject : BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }
    
    // нав. поля.
    public List<Groups> Groups { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}