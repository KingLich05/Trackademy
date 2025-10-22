using Trackademy.Application.AssignmentServices.Models;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.AssignmentServices;

public interface IAssignmentService : IBaseService<Assignment, AssignmentDto, AssignmentAddModel, AssignmentUpdateModel>
{
    Task<PagedResult<AssignmentDto>> GetAllAsync(GetAssignmentsRequest request, Guid userId);
    Task<Guid> CreateAsync(AssignmentAddModel model, Guid userId);
    Task<Guid> UpdateAsync(Guid id, AssignmentUpdateModel model, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}