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
    public async Task CreateUser(string name) // удалить, перегрузить базовый метод
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = name,
            PasswordHash = name,
            PhotoPath = name,
            CreatedDate = DateTime.UtcNow
        };
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<UserDto>> GetUsers()
    {
        var users = await dbContext.Users.ToListAsync();
        var userDtos = users
            .Select(u => new UserDto 
            {
                Id = u.Id,
                Name = u.FullName,
                Email = u.Email,
                PhotoPath = u.PhotoPath,
                Role = u.Role,
            })
            .ToList();
        return userDtos;
    }
}