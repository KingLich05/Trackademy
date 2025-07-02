using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Users.Interfaces;

namespace Trackademy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserServices service) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await service.GetById(id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser(
        [FromBody] string name)
    {
        await service.CreateUser(name);
        return Ok();
    }

    [HttpGet("get-users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await service.GetUsers();
        return Ok(users);
    }
}