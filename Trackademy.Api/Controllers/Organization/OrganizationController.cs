using Microsoft.AspNetCore.Authorization;
using Trackademy.Api.BaseController;
using Trackademy.Application.OrganizationServices;
using Trackademy.Application.OrganizationServices.Models;

namespace Trackademy.Api.Controllers.Organization;

[Authorize(Roles = "Admin")]
public class OrganizationController(IOrganizationService service) :
    BaseCrudController<Domain.Users.Organization, OrganizationDto, OrganizationAddModel, OrganizationAddModel>(service);