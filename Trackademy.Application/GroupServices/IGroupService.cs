using Trackademy.Application.GroupServices.Models;

namespace Trackademy.Application.GroupServices;

public interface IGroupService
{
    Task<List<GroupsTdo>> GetAllAsync(GroupRequest model);
    
    Task CreateGroup(GroupsAddModel model);

}