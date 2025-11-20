namespace Trackademy.Application.SubjectServices.Models;

public class SubjectMinimalViewModel
{
    public Guid SubjectId { get; set; }
    
    public string SubjectName { get; set; }
    
    public int? Price { get; set; }
}