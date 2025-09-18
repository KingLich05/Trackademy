using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.GroupServices;
using Trackademy.Application.GroupServices.Models;

namespace Trackademy.Api.Controllers.Group;

public class GroupController(IGroupService service) : ControllerBase
{
    [HttpPost("get-groups")]
    public async Task<IActionResult> GetGroups(
        [FromBody] GroupRequest getGroupRequest)
    {
        var groups = await service.GetAllAsync(getGroupRequest);
        return Ok(groups);
    }

    [HttpPost("create-group")]
    public async Task<IActionResult> CreateGroup(
    [FromBody] GroupsAddModel addGroupModel)
    {
        await service.CreateGroup(addGroupModel);
        return Ok();
    }
}