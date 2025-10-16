using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Lessons;
using Trackademy.Application.Lessons.Models;
using Trackademy.Application.Shared.Exception;
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
        Guid id,
        [FromBody] LessonStatusUpdateModel model)
    {
        try
        {
            var updated = await service.UpdateLessonStatusAsync(id, model.LessonStatus);
            if (!updated)
                return NotFound("Урок не найден");

            return Ok();
        }
        catch (ConflictException ex)
        {
            return BadRequest(ex.Message);
        }
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