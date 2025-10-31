using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Application.authenticator.Models;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IUserServices service) : ControllerBase
{
    
    [HttpGet("GetUserById/{id:guid}")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetUserById(
        Guid id)
    {
        var user = await service.GetById(id);
        
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("create")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await service.CreateUser(request);

        if (!result.IsSuccess)
        {
            if (result.ErrorMessage?.Contains("уже существует") == true)
            {
                return Conflict(result.ErrorMessage);
            }
            return BadRequest(result.ErrorMessage);
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.User!.Id }, result.User);
    }

    [HttpPost("get-users")]
    [RoleAuthorization(RoleEnum.Teacher)]
    public async Task<IActionResult> GetUsers(
        [FromBody] GetUserRequest getUserRequest)
    {
        var users = await service.GetUsers(getUserRequest);
        return Ok(users);
    }

    [HttpPut("update-user/{id:guid}")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UserUpdateModel getUserRequest)
    {
        var currentUserIdClaim = User.FindFirst("Id");
        if (currentUserIdClaim == null)
            return Unauthorized("Не удалось определить текущего пользователя");

        if (!Guid.TryParse(currentUserIdClaim.Value, out var currentUserId))
            return Unauthorized("Неверный ID пользователя");

        var userRole = User.FindFirst("Role")?.Value;

        if (userRole == RoleEnum.Administrator.ToString() || userRole == RoleEnum.Owner.ToString())
        {
            var result = await service.UpdateUser(id, getUserRequest);
            return Ok(result);
        }

        if (userRole == RoleEnum.Teacher.ToString() || userRole == RoleEnum.Student.ToString())
        {
            if (currentUserId != id)
            {
                return Forbid("Вы можете редактировать только свой профиль");
            }

            var result = await service.UpdateUser(id, getUserRequest);
            return Ok(result);
        }

        return Forbid("Недостаточно прав для выполнения операции");
    }

    [HttpDelete("{id:guid}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> DeleteUser(
        Guid id)
    {
        var result = await service.DeleteUser(id);

        if (!result) return NotFound();

        return Ok(result);
    }
}