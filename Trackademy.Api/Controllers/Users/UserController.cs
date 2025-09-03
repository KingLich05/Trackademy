using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;

namespace Trackademy.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserServices service) : ControllerBase
{
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(
        [FromHeader] Guid id)
    {
        // var user = await service.GetById(id);
        //
        // if (user == null)
        //     return NotFound();

        return Ok();
    }

    [HttpPost("get-users")]
    public async Task<IActionResult> GetUsers(
        [FromBody] GetUserRequest getUserRequest)
    {
        var users = await service.GetUsers(getUserRequest);
        return Ok(users);
    }
}