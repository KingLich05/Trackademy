using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Api.BaseController;
using Trackademy.Application.SubjectServices;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers.SubjectDir;

[Authorize]
public class SubjectController(ISubjectService service) :
    BaseCrudController<Subject, SubjectDto, SubjectAddModel,SubjectUpdateModel>(service)
{
    [HttpPost("GetAllSubjects")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetAllSubjects([FromBody] GetSubjectsRequest request)
    {
        var result = await service.GetAllAsync(request);
        return Ok(result);
    }

    [NonAction]
    [HttpGet]
    [RoleAuthorization(RoleEnum.Administrator)]
    public override async Task<IActionResult> GetAll()
    {
        return await base.GetAll();
    }

    [HttpGet("{id}")]
    [RoleAuthorization(RoleEnum.Student)]
    public override async Task<IActionResult> GetById(Guid id)
    {
        return await base.GetById(id);
    }

    [HttpPost("create")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public override async Task<IActionResult> Create([FromBody] SubjectAddModel dto)
    {
        return await base.Create(dto);
    }

    [HttpPut("{id}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public override async Task<IActionResult> Update(Guid id, [FromBody] SubjectUpdateModel dto)
    {
        return await base.Update(id, dto);
    }

    [HttpDelete("{id}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public override async Task<IActionResult> Delete(Guid id)
    {
        return await base.Delete(id);
    }
}