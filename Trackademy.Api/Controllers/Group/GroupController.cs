using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.BaseController;
using Trackademy.Application.GroupServices;
using Trackademy.Application.GroupServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers.Group;

public class GroupController(IGroupService service) :
    BaseCrudController<Groups, GroupsDto, GroupsAddModel>(service)
{
    [HttpGet("get-groups")]
    public async Task<IActionResult> GetGroups(
        [FromQuery] Guid organizationId)
    {
        var groups = await service.GetAllAsync(organizationId);
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