using Trackademy.Application.authenticator.Models;
using Trackademy.Application.Shared.Models;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Users.Interfaces;

public interface IUserServices
{
    Task<PagedResult<UserDto>> GetUsers(GetUserRequest getUserRequest);
    
    Task<UserByIdDto> GetById(Guid id);
    
    Task<UserCreationResult> CreateUser(CreateUserRequest request);
    
    Task<Guid> UpdateUser(Guid id, UserUpdateModel updateUserRequest);
    
    Task<bool> UpdatePassword(UpdatePasswordRequest request);
    
    Task<UserImportResult> ImportUsersFromExcel(List<UserImportRow> rows, Guid organizationId);
    
    byte[] GenerateImportTemplate();
    
    Task<bool> DeleteUser(Guid id);
}