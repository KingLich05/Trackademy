using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.BaseController;
using Trackademy.Application.Shared.Models;
using Trackademy.Application.SubjectServices;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers.SubjectDir;

public class SubjectController(ISubjectService service) :
    BaseCrudController<Subject, SubjectDto, SubjectAddModel>(service)
{
    [HttpGet("GetAllSubjects")]
    public new async Task<IActionResult> GetAllSubjects(
        [FromQuery] RequestIdOrganization request)
    {
        var items = await service.GetAllAsync(request);
        return Ok(items);
    }
}