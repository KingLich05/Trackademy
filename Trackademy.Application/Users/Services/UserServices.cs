using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.Services;

public class UserServices(TrackademyDbContext dbContext, IMapper mapper) : 
    BaseService<User, UserDto, AddUserModel>(dbContext, mapper),
    IUserServices
{
    public async Task<List<UserDto>> GetUsers(GetUserRequest getUserRequest)
    {
        var usersQuery = dbContext.Users.Include(x => x.Groups).AsQueryable();

        if (getUserRequest.search != null)
        {
            usersQuery = usersQuery.Where(x => x.FullName.Contains(getUserRequest.search));
        }

        if (getUserRequest.RoleIds != null)
        {
            usersQuery = usersQuery.Where(x => getUserRequest.RoleIds.Contains(x.Role));
        }

        if (getUserRequest.GroupIds != null || getUserRequest.GroupIds.Count > 0)
        {
            usersQuery = usersQuery.Where(x => 
                x.Groups.Any(g => getUserRequest.GroupIds.Contains(g.Id)));
        }
        
        var users = await usersQuery.ProjectTo<UserDto>(mapper.ConfigurationProvider).ToListAsync();
        
        return users;
    }
}