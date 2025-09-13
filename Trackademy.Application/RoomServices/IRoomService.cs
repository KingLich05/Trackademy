using Trackademy.Application.RoomServices.Models;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Domain.Users;

namespace Trackademy.Application.RoomServices;

public interface IRoomService : IBaseService<Room, RoomDto, RoomAddModel>;