using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.Services;

public class UserServices(TrackademyDbContext dbContext, IMapper mapper) : 
    BaseService<User, UserDto>(dbContext, mapper),
    IUserServices
{
    public async Task<User?> GetById(Guid id)
    { 
        return await dbContext.Users.FindAsync(id);
    }

    public async Task CreateUser(string name)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = name,
            PasswordHash = name,
            PhotoPath = name,
            CreatedDate = DateTime.UtcNow,
            Role = null,
            RoleId = Guid.Empty
        };
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<User>> GetUsers()
    {
        return await dbContext.Users.ToListAsync();
    }
}