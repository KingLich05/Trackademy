using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;

namespace Trackademy.Application.Users.Services;

public class UserServices(TrackademyDbContext dbContext, IMapper mapper) :
    IUserServices
{
    public async Task<List<UserDto>> GetUsers(GetUserRequest getUserRequest)
    {
        var usersQuery = dbContext.Users
            .Include(x => x.Groups)
            .AsQueryable();

        if (getUserRequest.search is { Length: > 0 } && !string.IsNullOrWhiteSpace(getUserRequest.search))
        {
            usersQuery = usersQuery.Where(x => x.FullName.Contains(getUserRequest.search));
        }

        if (getUserRequest.RoleIds != null && getUserRequest.RoleIds.Count != 0)
        {
            usersQuery = usersQuery.Where(x => getUserRequest.RoleIds.Contains(x.Role));
        }

        if (getUserRequest.GroupIds != null && getUserRequest.GroupIds.Count != 0)
        {
            usersQuery = usersQuery.Where(x => 
                x.Groups.Any(g => getUserRequest.GroupIds.Contains(g.Id)));
        }

        var users = await usersQuery.ProjectTo<UserDto>(mapper.ConfigurationProvider).ToListAsync();

        return users;
    }

    public async Task<UserByIdDto> GetById(Guid id)
    {
        var user = await dbContext.Users
            .Include(x => x.Payments)
            .Include(x => x.Groups)
            .FirstOrDefaultAsync(x => x.Id == id);
        
        return mapper.Map<UserByIdDto>(user);
    }
}