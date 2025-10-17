using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.authenticator.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.Exception;
using Trackademy.Application.Shared.Extensions;
using Trackademy.Application.Shared.Models;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Users.Services;

public class UserServices(TrackademyDbContext dbContext, IMapper mapper) :
    IUserServices
{
    public async Task<PagedResult<UserDto>> GetUsers(GetUserRequest getUserRequest)
    {
        var usersQuery = dbContext.Users
            .Where(x => x.Role != RoleEnum.Administrator && x.OrganizationId == getUserRequest.OrganizationId)
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

        var pagedUsers = await usersQuery
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .OrderBy(x => x.Name)
            .ToPagedResultAsync(getUserRequest);

        return pagedUsers;
    }

    public async Task<UserByIdDto> GetById(Guid id)
    {
        var user = await dbContext.Users
            .Include(x => x.Payments)
            .Include(x => x.Groups)
            .FirstOrDefaultAsync(x => x.Id == id);

        return mapper.Map<UserByIdDto>(user);
    }

    public async Task<Guid> UpdateUser(Guid id, CreateUserRequest request)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            throw new ConflictException("Пользователя с таким идентификатором не существует");
        }

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.ParentPhone = request.ParentPhone;
        user.Role = request.Role;
        user.Birthday = request.Birthday;

        await dbContext.SaveChangesAsync();
        return user.Id;
    }

    public async Task<bool> DeleteUser(Guid id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return false;
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
        return true;
    }
}