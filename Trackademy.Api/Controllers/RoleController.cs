using Trackademy.Application.Roles.Interface;
using Trackademy.Application.Roles.Models;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers;

public class RoleController(IRoleService service) :
    BaseCrudController<Roles, RoleDto, AddRoleModel>(service);