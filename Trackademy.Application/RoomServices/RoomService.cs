using AutoMapper;
using Trackademy.Application.Persistance;
using Trackademy.Application.RoomServices.Models;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Domain.Users;

namespace Trackademy.Application.RoomServices;

public class RoomService : 
    BaseService<Room, RoomDto, RoomAddModel>,
    IRoomService
{
    public RoomService(TrackademyDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
    }

    public override Task<RoomDto> CreateAsync(RoomAddModel dto)
    {
        if (dto.Capacity != null && dto.Capacity == 0)
        {
            return null;
        }

        return base.CreateAsync(dto);
    }
}