using Trackademy.Application.authenticator.Models;
using Trackademy.Application.Shared.Models;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.Interfaces;

public interface IUserServices
{
    Task<PagedResult<UserDto>> GetUsers(GetUserRequest getUserRequest);
    
    Task<UserByIdDto> GetById(Guid id);
    
    Task<Guid> UpdateUser(Guid id, CreateUserRequest updateUserRequest);
    
    Task<bool> DeleteUser(Guid id);
}