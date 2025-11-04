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
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.Services;

public class UserServices(TrackademyDbContext dbContext, IMapper mapper) :
    IUserServices
{
    public async Task<PagedResult<UserDto>> GetUsers(GetUserRequest getUserRequest)
    {
        var usersQuery = dbContext.Users
            .Where(x => x.Role != RoleEnum.Administrator 
                        && x.Role != RoleEnum.Owner
                        && x.OrganizationId == getUserRequest.OrganizationId)
            .Include(x => x.Groups)
            .AsQueryable();

        if (getUserRequest.search is { Length: > 0 } && !string.IsNullOrWhiteSpace(getUserRequest.search))
        {
            usersQuery = usersQuery.Where(x =>
                x.FullName.ToLower().Contains(getUserRequest.search.ToLower()) ||
                x.Login.ToLower().Contains(getUserRequest.search.ToLower())
            );
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

    public async Task<UserCreationResult> CreateUser(CreateUserRequest request)
    {
        if (!ValidateData(request))
        {
            throw new ConflictException("Не все поля заполнены.");
        }

        if (!VerifyNullEmailAndPassword(request.Email, request.Password))
        {
            throw new ConflictException("Email и пароль обязательны");
        }

        var organization = await dbContext.Organizations
            .Where(x => x.Id == request.OrganizationId)
            .FirstOrDefaultAsync();

        if (organization == null)
        {
            throw new ConflictException("Ошибка с организацией.");
        }

        var exists = await dbContext.Users
            .Where(x => x.OrganizationId == request.OrganizationId)
            .AnyAsync(u => u.Login == request.Login);

        if (exists)
        {
            throw new ConflictException("Пользователь с таким login уже существует.");
        }

        var user = new User
        {
            Login = request.Login,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            ParentPhone = request.ParentPhone,
            Role = request.Role,
            Birthday = request.Birthday,
            CreatedDate = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            OrganizationId = request.OrganizationId,
            Organization = organization,
            IsTrial = request.IsTrial
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return new UserCreationResult
        {
            IsSuccess = true,
            User = new UserCreatedDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Login = user.Login,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        };
    }

    public async Task<Guid> UpdateUser(Guid id, UserUpdateModel request)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            throw new ConflictException("Пользователя с таким идентификатором не существует");
        }
        var exists = await dbContext.Users
            .AnyAsync(u => u.Login == request.Login && u.Id != id);

        if (exists)
        {
            throw new ConflictException("Такой логин уже существует.");
        }

        user.Login = request.Login;
        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.ParentPhone = request.ParentPhone;
        user.Role = request.Role;
        user.Birthday = request.Birthday;
        user.IsTrial = request.IsTrial;

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

    #region Private methods

    private bool ValidateData(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName)) return false;
        if (string.IsNullOrWhiteSpace(request.Phone)) return false;
        if (string.IsNullOrWhiteSpace(request.Password)) return false;
        if (!Enum.IsDefined(typeof(RoleEnum), request.Role)) return false;

        return true;
    }

    private bool VerifyNullEmailAndPassword(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        return true;
    }

    #endregion
}