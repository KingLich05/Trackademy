using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Lessons;
using Trackademy.Application.Lessons.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers.Schedule;

[ApiController]
[Route("api/[controller]")]
public class LessonController(ILessonService service) : ControllerBase
{
    [HttpPatch("{id}/reschedule")]
    public async Task<IActionResult> RescheduleLesson(
        Guid id,
        [FromBody] LessonRescheduleModel model)
    {
        var result = await service.RescheduleLessonAsync(id, model);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpPost("custom")]
    public async Task<IActionResult> CreateCustomLesson(
        [FromBody] LessonCustomAddModel model)
    {
        var id = await service.CreateCustomLessonAsync(model);
        return Ok(id);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateLessonStatus(
        Guid id)
    { //TODO: здесь подумать каким образом именно будет меняться статус так как нужно будет в будущем еще проставлять attendance
        var updated = await service.UpdateLessonStatusAsync(id, LessonStatus.Completed);
        if (!updated)
            return NotFound();

        return Ok();
    }

    [HttpGet("by-schedule/{scheduleId}")]
    public async Task<IActionResult> GetLessonsBySchedule(
        Guid scheduleId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate)
    {
        var result = await service.GetLessonsByScheduleAsync(scheduleId, fromDate, toDate);
        return Ok(result);
    }
    
    /// <summary>
    /// Получение урока по ID
    /// </summary>
    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> GetLessonById(Guid id)
    {
        var result = await service.GetLessonByIdAsync(id);
        return Ok(result);
    }
}