using AutoMapper;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.AutoMapper;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<Groups, GroupMinimalViewModel>();
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Groups, opt => opt.MapFrom(x => x.Groups))
            .ForMember(x => x.Name, opt => opt.MapFrom(q => q.FullName));
    }
}