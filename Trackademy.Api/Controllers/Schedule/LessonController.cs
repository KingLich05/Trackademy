using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Application.Lessons;
using Trackademy.Application.Lessons.Models;
using Trackademy.Application.Shared.Exception;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers.Schedule;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LessonController(ILessonService service) : ControllerBase
{
    [HttpPatch("{id}/moved")]
    [RoleAuthorization(RoleEnum.Teacher)]
    public async Task<IActionResult> MoveLesson(
        Guid id,
        [FromBody] LessonRescheduleModel model)
    {
        var result = await service.RescheduleLessonAsync(id, model);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpPost("custom")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> CreateCustomLesson(
        [FromBody] LessonCustomAddModel model)
    {
        var id = await service.CreateCustomLessonAsync(model);
        return Ok(id);
    }

    [HttpPatch("{id}/cancel")]
    [RoleAuthorization(RoleEnum.Teacher)]
    public async Task<IActionResult> CancelLessonStatus(
        Guid id,
        [FromBody] LessonStatusUpdateModel model)
    {
        try
        {
            var updated = await service.UpdateLessonStatusAsync(id, model.LessonStatus, model.CancelReason);
            if (!updated)
                return NotFound("Урок не найден");

            return Ok();
        }
        catch (ConflictException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("by-schedule")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetLessonsBySchedule([FromBody] GetLessonsByScheduleRequest request)
    {
        var result = await service.GetLessonsByScheduleAsync(request);
        return Ok(result);
    }
    
    /// <summary>
    /// Получение урока по ID
    /// </summary>
    [HttpGet("[action]/{id}")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetLessonById(Guid id)
    {
        var result = await service.GetLessonByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Добавление заметки к уроку
    /// </summary>
    [HttpPatch("{id}/note")]
    [RoleAuthorization(RoleEnum.Teacher)]
    public async Task<IActionResult> UpdateLessonNote(
        Guid id,
        [FromBody] LessonNoteModel model)
    {
        var updated = await service.UpdateLessonNoteAsync(id, model.Note);
        if (!updated)
            return NotFound("Урок не найден");

        return Ok();
    }
}