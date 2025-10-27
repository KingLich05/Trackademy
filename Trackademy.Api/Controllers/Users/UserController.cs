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
[RoleAuthorization(RoleEnum.Administrator)]
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
        var result = await service.UpdateUser(id, getUserRequest);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(
        Guid id)
    {
        var result = await service.DeleteUser(id);

        if (!result) return NotFound();

        return Ok(result);
    }
}