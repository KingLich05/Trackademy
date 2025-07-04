using AutoMapper;
using Trackademy.Application.Persistance;
using Trackademy.Application.Roles.Interface;
using Trackademy.Application.Roles.Models;
using Trackademy.Application.Shared;

namespace Trackademy.Application.Roles.Service;

public class RoleService(TrackademyDbContext dbContext, IMapper mapper) :
    BaseService<Domain.Users.Roles, RoleDto>(dbContext, mapper),
    IRoleService
{
    
}