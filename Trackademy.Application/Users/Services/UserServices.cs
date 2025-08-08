using AutoMapper;
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

    public async Task<List<User>> GetUsers()
    {
        return await dbContext.Users.ToListAsync();
    }
}