using Trackademy.Application.GroupServices.Models;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.GroupServices;

public interface IGroupService : IBaseService<Groups, GroupsDto, GroupsAddModel, GroupsUpdateModel>
{
    Task<List<GroupsDto>> GetAllAsync(Guid model);
    
    Task<PagedResult<GroupsDto>> GetAllAsync(GetGroupsRequest request);
    
    Task<Guid> CreateGroup(GroupsAddModel model);
    
    Task FreezeStudentAsync(FreezeStudentRequest request);
    
    Task UnfreezeStudentAsync(UnfreezeStudentRequest request);
}