using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.BaseController;
using Trackademy.Application.GroupServices;
using Trackademy.Application.GroupServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers.Group;

public class GroupController(IGroupService service) :
    BaseCrudController<Groups, GroupsDto, GroupsAddModel, GroupsUpdateModel>(service)
{
    [HttpPost("get-groups")]
    public async Task<IActionResult> GetGroups([FromBody] GetGroupsRequest request)
    {
        var groups = await service.GetAllAsync(request);
        return Ok(groups);
    }

    [HttpPost("create-group")]
    public async Task<IActionResult> CreateGroup(
    [FromBody] GroupsAddModel addGroupModel)
    {
        var result = await service.CreateGroup(addGroupModel);
        return Ok(result);
    }
    
    [NonAction]
    public override Task<IActionResult> Create([FromBody] GroupsAddModel dto)
    {
        return base.Create(dto);
    }
    
    [NonAction]
    public override async Task<IActionResult> GetAll()
    {
        return await base.GetAll();
    }
}