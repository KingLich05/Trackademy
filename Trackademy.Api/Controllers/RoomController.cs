using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.BaseController;
using Trackademy.Application.RoomServices;
using Trackademy.Application.RoomServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers;

public class RoomController(IRoomService service) :
    BaseCrudController<Room, RoomDto, RoomAddModel, RoomUpdateModel>(service)
{
    [HttpPost("GetAllRooms")]
    public async Task<IActionResult> GetAllRooms([FromBody] GetRoomsRequest request)
    {
        var result = await service.GetAllAsync(request);
        return Ok(result);
    }
    
    [NonAction]
    public override async Task<IActionResult> GetAll()
    {
        return await base.GetAll();
    }
}