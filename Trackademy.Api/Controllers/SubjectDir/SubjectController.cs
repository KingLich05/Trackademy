using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.BaseController;
using Trackademy.Application.SubjectServices;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers.SubjectDir;

[Authorize]
public class SubjectController(ISubjectService service) :
    BaseCrudController<Subject, SubjectDto, SubjectAddModel,SubjectUpdateModel>(service)
{
    [HttpPost("GetAllSubjects")]
    public async Task<IActionResult> GetAllSubjects([FromBody] GetSubjectsRequest request)
    {
        var result = await service.GetAllAsync(request);
        return Ok(result);
    }

    [NonAction]
    public override async Task<IActionResult> GetAll()
    {
        return await base.GetAll();
    }
}