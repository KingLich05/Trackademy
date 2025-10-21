using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Application.Schedule;
using Trackademy.Application.Schedule.Model;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers.Schedule;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScheduleController(IScheduleService service) : ControllerBase
{
    [HttpGet("[action]/{id}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<ScheduleViewModel?> GetScheduleById(Guid id)
    {
        var result = await service.GetSchedule(id);
        return result;
    }

    [HttpPost("create-schedule")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> CreateSchedule(
        [FromBody] ScheduleAddModel addModel)
    {
        var result = await service.CreateSchedule(addModel);
        return Ok(result);
    }

    [HttpPut("update-schedule/{id}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> UpdateSchedule(
        Guid id,
        [FromBody] ScheduleUpdateModel model)
    {
        var result = await service.UpdateScheduleAsync(id, model);
        return Ok(result);
    }

    [HttpPost("get-all-schedules")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetAllSchedulesAsync(
        [FromBody] ScheduleRequest request)
    {
        var result = await service.GetAllSchedulesAsync(request);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.DeleteAsync(id);

        if (!result) return NotFound();

        return Ok();
    }
}