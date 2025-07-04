using AutoMapper;
using Trackademy.Application.Roles.Models;

namespace Trackademy.Application.Roles.AutoMapper;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Domain.Users.Roles, RoleDto>();
    }
}