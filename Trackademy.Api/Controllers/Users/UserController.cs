using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.authenticator.Models;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;

namespace Trackademy.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
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

    [HttpPost("get-users")]
    public async Task<IActionResult> GetUsers(
        [FromBody] GetUserRequest getUserRequest)
    {
        var users = await service.GetUsers(getUserRequest);
        return Ok(users);
    }

    [HttpPut("update-user")]
    public async Task<IActionResult> UpdateUser(
        [FromHeader] Guid id,
        [FromBody] CreateUserRequest getUserRequest)
    {
        return Ok(await service.UpdateUser(id, getUserRequest));
    }
}