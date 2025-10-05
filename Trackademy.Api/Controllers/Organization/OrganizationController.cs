using Trackademy.Api.BaseController;
using Trackademy.Application.OrganizationServices;
using Trackademy.Application.OrganizationServices.Models;

namespace Trackademy.Api.Controllers.Organization;

public class OrganizationController(IOrganizationService service) :
    BaseCrudController<Domain.Users.Organization, OrganizationDto, OrganizationAddModel, OrganizationAddModel>(service);