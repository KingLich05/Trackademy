using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Api.BaseController;
using Trackademy.Application.OrganizationServices;
using Trackademy.Application.OrganizationServices.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers.Organization;

[Authorize]
[RoleAuthorization(RoleEnum.Owner)]
public class OrganizationController(IOrganizationService service) :
    BaseCrudController<Domain.Users.Organization, OrganizationDto, OrganizationAddModel, OrganizationAddModel>(service)
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null)
    {
        var organizations = await service.GetAllWithSearchAsync(search);
        return Ok(organizations);
    }
}