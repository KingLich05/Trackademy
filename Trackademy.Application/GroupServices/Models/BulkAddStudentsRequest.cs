namespace Trackademy.Application.GroupServices.Models;

public class BulkAddStudentsRequest
{
    public Guid GroupId { get; set; }
    public List<Guid> StudentIds { get; set; } = new();
}
