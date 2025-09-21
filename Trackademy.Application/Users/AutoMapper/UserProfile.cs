using AutoMapper;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.AutoMapper;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<Groups, GroupMinimalViewModel>()
            .ForMember(x => x.Id, o => o.MapFrom(q => q.Id))
            .ForMember(x => x.Name, o => o.MapFrom(q => q.Name));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Groups, opt => opt.MapFrom(x => x.Groups))
            .ForMember(x => x.Name, opt => opt.MapFrom(q => q.FullName));
        
        CreateMap<User, UserByIdDto>()
            .ForMember(dest => dest.Groups, opt => opt.MapFrom(x => x.Groups))
            .ForMember(x => x.Name, opt => opt.MapFrom(q => q.FullName));
    }
}