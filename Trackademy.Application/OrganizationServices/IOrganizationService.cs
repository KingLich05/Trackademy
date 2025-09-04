using Trackademy.Application.OrganizationServices.Models;
using Trackademy.Application.Shared.BaseCrud;

namespace Trackademy.Application.OrganizationServices;

public interface IOrganizationService : IBaseService<Domain.Users.Organization, OrganizationDto, OrganizationAddModel>;