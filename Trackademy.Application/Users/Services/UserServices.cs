using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.Services;

public class UserServices(TrackademyDbContext dbContext) : IUserServices
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
            Username = name
        };
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<User>> GetUsers()
    {
        return await dbContext.Users.ToListAsync();
    }
}