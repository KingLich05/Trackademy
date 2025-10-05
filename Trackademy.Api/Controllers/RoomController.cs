using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.BaseController;
using Trackademy.Application.RoomServices;
using Trackademy.Application.RoomServices.Models;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers;

public class RoomController(IRoomService service) :
    BaseCrudController<Room, RoomDto, RoomAddModel, RoomUpdateModel>(service)
{
    [HttpGet("GetAllRooms")]
    public new async Task<IActionResult> GetAllRooms(
        [FromQuery] RequestIdOrganization request)
    {
        var items = await service.GetAllAsync(request);
        return Ok(items);
    }
}