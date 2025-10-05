namespace Trackademy.Application.RoomServices.Models;

public class RoomUpdateModel
{
    public string Name { get; set; }
    
    public required int Capacity { get; set; }
}