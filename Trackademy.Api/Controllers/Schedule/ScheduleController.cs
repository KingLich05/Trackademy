using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Schedule;
using Trackademy.Application.Schedule.Model;

namespace Trackademy.Api.Controllers.Schedule;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController(IScheduleService service) : ControllerBase
{
    [HttpPost("create-schedule")]
    public async Task<IActionResult> CreateSchedule(
        [FromBody] ScheduleAddModel addModel)
    {
        var result = await service.CreateSchedule(addModel);
        return Ok(result);
    }

    [HttpPost("get-all-schedules")]
    public async Task<IActionResult> GetAllSchedulesAsync(
        [FromBody] ScheduleRequest request)
    {
        var result = await service.GetAllSchedulesAsync(request);

        return Ok(result);
    }
}