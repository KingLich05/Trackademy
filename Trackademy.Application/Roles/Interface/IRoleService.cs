using Trackademy.Application.Roles.Models;
using Trackademy.Application.Shared;
using Trackademy.Application.Shared.BaseCrud;

namespace Trackademy.Application.Roles.Interface;

public interface IRoleService : IBaseService<Domain.Users.Roles, RoleDto, AddRoleModel>;