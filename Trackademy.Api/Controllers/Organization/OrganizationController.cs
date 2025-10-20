using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.BaseController;
using Trackademy.Application.OrganizationServices;
using Trackademy.Application.OrganizationServices.Models;

namespace Trackademy.Api.Controllers.Organization;

[Authorize(Roles = "Admin")]
public class OrganizationController(IOrganizationService service) :
    BaseCrudController<Domain.Users.Organization, OrganizationDto, OrganizationAddModel, OrganizationAddModel>(service)
{
    [AllowAnonymous]
    public override async Task<IActionResult> GetAll()
    {
        return await base.GetAll();
    }
}