using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.Interfaces;

public interface IUserServices
{
    Task CreateUser(string name);
    Task<List<UserDto>> GetUsers();
}