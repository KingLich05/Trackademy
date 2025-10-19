using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.authenticator.Models;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;

namespace Trackademy.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IUserServices service) : ControllerBase
{
    
    [HttpGet("GetUserById")]
    public async Task<IActionResult> GetUserById(
        [FromQuery] Guid id)
    {
        var user = await service.GetById(id);
        
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
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
    public async Task<IActionResult> GetUsers(
        [FromBody] GetUserRequest getUserRequest)
    {
        var users = await service.GetUsers(getUserRequest);
        return Ok(users);
    }

    [HttpPut("update-user")]
    public async Task<IActionResult> UpdateUser(
        [FromQuery] Guid id,
        [FromBody] CreateUserRequest getUserRequest)
    {
        var result = await service.UpdateUser(id, getUserRequest);
        return Ok(result);
    }

    [HttpDelete("delete-user")]
    public async Task<IActionResult> DeleteUser(
        [FromQuery] Guid id)
    {
        var result = await service.DeleteUser(id);

        if (!result) return NotFound();

        return Ok(result);
    }
}