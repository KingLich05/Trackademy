using AutoMapper;
using Trackademy.Application.RoomServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.RoomServices;

public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<Room, RoomDto>();
        CreateMap<RoomAddModel, Room>()
            .ForMember(x => x.Schedules, opt => opt.Ignore());
    }
}