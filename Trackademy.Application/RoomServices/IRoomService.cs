using Trackademy.Application.RoomServices.Models;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.RoomServices;

public interface IRoomService : IBaseService<Room, RoomDto, RoomAddModel, RoomUpdateModel>
{
    Task<PagedResult<RoomDto>> GetAllAsync(GetRoomsRequest request);
}