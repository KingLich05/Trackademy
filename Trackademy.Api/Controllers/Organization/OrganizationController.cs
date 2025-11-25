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
    [AllowAnonymous]
    public override async Task<IActionResult> GetAll()
    {
        return await base.GetAll();
    }
}