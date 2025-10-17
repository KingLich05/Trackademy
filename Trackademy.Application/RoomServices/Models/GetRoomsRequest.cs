using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.RoomServices.Models;

public class GetRoomsRequest : PagedRequest
{
    public Guid OrganizationId { get; set; }
}