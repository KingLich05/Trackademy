using Trackademy.Application.Roles.Models;
using Trackademy.Application.Shared;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers;

public class RoleController : BaseCrudController<Roles, RoleDto>
{
    protected RoleController(IBaseService<Roles, RoleDto> service) : base(service)
    {
    }
}