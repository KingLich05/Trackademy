using Trackademy.Api.BaseController;
using Trackademy.Application.RoomServices;
using Trackademy.Application.RoomServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers;

public class RoomController(IRoomService service) :
    BaseCrudController<Room, RoomDto, RoomAddModel>(service);