using Microsoft.AspNetCore.Mvc;

namespace Trackademy.Api.Controllers;

public class TestController : ControllerBase
{
    [HttpGet("api/test")]
    public async Task<IActionResult> ApiForLife()
    {
        Console.WriteLine("test-test");

        return Ok("все ок");
    }
}