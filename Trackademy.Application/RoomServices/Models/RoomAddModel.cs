namespace Trackademy.Application.RoomServices.Models;

public class RoomAddModel
{
    public string Name { get; set; }
    
    public int? Capacity { get; set; }
    
    public Guid OrganizationId { get; set; }
}