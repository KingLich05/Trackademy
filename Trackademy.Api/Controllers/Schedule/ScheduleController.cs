using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Schedule;
using Trackademy.Application.Schedule.Model;

namespace Trackademy.Api.Controllers.Schedule;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController(IScheduleService service) : ControllerBase
{
    [HttpGet("[action]/{id}")]
    public async Task<ScheduleViewModel?> GetScheduleById(Guid id)
    {
        var result = await service.GetSchedule(id);
        return result;
    }

    [HttpPost("create-schedule")]
    public async Task<IActionResult> CreateSchedule(
        [FromBody] ScheduleAddModel addModel)
    {
        var result = await service.CreateSchedule(addModel);
        return Ok(result);
    }

    [HttpPut("update-schedule/{id}")]
    public async Task<IActionResult> UpdateSchedule(
        Guid id,
        [FromBody] ScheduleUpdateModel model)
    {
        var result = await service.UpdateScheduleAsync(id, model);
        return Ok(result);
    }

    [HttpPost("get-all-schedules")]
    public async Task<IActionResult> GetAllSchedulesAsync(
        [FromBody] ScheduleRequest request)
    {
        var result = await service.GetAllSchedulesAsync(request);

        return Ok(result);
    }

    [HttpGet("get-all-lessons")]
    public async Task<IActionResult> GetAllLessonsAsync(
        [FromQuery] LessonRequest request)
    {
        var result = await service.GetAllLessons(request);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.DeleteAsync(id);

        if (!result) return NotFound();

        return Ok();
    }
}