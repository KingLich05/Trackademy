using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Api.BaseController;
using Trackademy.Application.GroupServices;
using Trackademy.Application.GroupServices.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers.Group;

[Authorize]
public class GroupController(IGroupService service) :
    BaseCrudController<Groups, GroupsDto, GroupsAddModel, GroupsUpdateModel>(service)
{
    [HttpPost("get-groups")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetGroups([FromBody] GetGroupsRequest request)
    {
        var groups = await service.GetAllAsync(request);
        return Ok(groups);
    }

    [HttpPost("create-group")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> CreateGroup(
    [FromBody] GroupsAddModel addGroupModel)
    {
        var result = await service.CreateGroup(addGroupModel);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [RoleAuthorization(RoleEnum.Student)]
    public override async Task<IActionResult> GetById(Guid id)
    {
        return await base.GetById(id);
    }

    [HttpPut("{id}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public override async Task<IActionResult> Update(Guid id, [FromBody] GroupsUpdateModel dto)
    {
        return await base.Update(id, dto);
    }

    [HttpDelete("{id}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public override async Task<IActionResult> Delete(Guid id)
    {
        return await base.Delete(id);
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
    
    [HttpPost("freeze-student")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> FreezeStudent([FromBody] FreezeStudentRequest request)
    {
        await service.FreezeStudentAsync(request);
        return Ok("Студент успешно заморожен.");
    }
    
    [HttpPost("unfreeze-student")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> UnfreezeStudent([FromBody] UnfreezeStudentRequest request)
    {
        await service.UnfreezeStudentAsync(request);
        return Ok("Студент успешно разморожен, платеж продлен.");
    }
}