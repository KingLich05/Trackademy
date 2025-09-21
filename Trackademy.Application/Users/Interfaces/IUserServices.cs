using Trackademy.Application.authenticator.Models;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.Interfaces;

public interface IUserServices
{
    Task<List<UserDto>> GetUsers(GetUserRequest getUserRequest);
    
    Task<UserByIdDto> GetById(Guid id);
    
    Task<bool> UpdateUser(Guid id, CreateUserRequest updateUserRequest);
    
    Task<bool> DeleteUser(Guid id);
}