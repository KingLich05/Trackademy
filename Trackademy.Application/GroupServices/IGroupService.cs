using Trackademy.Application.GroupServices.Models;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Domain.Users;

namespace Trackademy.Application.GroupServices;

public interface IGroupService : IBaseService<Groups, GroupsDto, GroupsAddModel, GroupsUpdateModel>
{
    Task<List<GroupsDto>> GetAllAsync(Guid model);
    
    Task<Guid> CreateGroup(GroupsAddModel model);

}