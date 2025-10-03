namespace Trackademy.Application.RoomServices.Models;

public class RoomAddModel
{
    public string Name { get; set; }
    
    public required int Capacity { get; set; }
    
    public Guid OrganizationId { get; set; }
}