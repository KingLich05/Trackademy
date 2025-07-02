using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.Interfaces;

public interface IUserServices
{
    Task<User?> GetById(Guid id);
    Task CreateUser(string name);
    Task<List<User>> GetUsers();
}