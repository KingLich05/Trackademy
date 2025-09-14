namespace Trackademy.Application.RoomServices.Models;

public class RoomDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public int? Capacity { get; set; }

    public Guid OrganizationId { get; set; }
}