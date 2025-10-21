using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Application.Attendances;
using Trackademy.Application.Attendances.Models;
using Trackademy.Application.Shared.Exception;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers.Attendanc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    /// <summary>
    /// Массовая отметка посещаемости для урока
    /// </summary>
    [HttpPost("mark-bulk")]
    [RoleAuthorization(RoleEnum.Teacher)]
    public async Task<IActionResult> MarkAttendancesBulk([FromBody] AttendanceBulkCreateModel model)
    {
        try
        {
            var result = await _attendanceService.MarkAttendancesAsync(model);
            return Ok(new { success = result });
        }
        catch (ConflictException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Обновление статуса посещаемости для конкретного студента
    /// </summary>
    [HttpPut("update")]
    [RoleAuthorization(RoleEnum.Teacher)]
    public async Task<IActionResult> UpdateAttendance([FromBody] AttendanceUpdateModel model)
    {
        var result = await _attendanceService.UpdateAttendanceAsync(model);
        if (result == null)
            return NotFound("Запись о посещаемости не найдена");

        return Ok(result);
    }

    /// <summary>
    /// Получение записи посещаемости по ID
    /// </summary>
    [HttpGet("{id}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> GetAttendanceById(Guid id)
    {
        var result = await _attendanceService.GetAttendanceByIdAsync(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Получение списка посещаемости с фильтрацией и пагинацией
    /// </summary>
    [HttpPost("get-all-attendances")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetAttendances([FromBody] AttendanceFilterModel filter)
    {
        try
        {
            var result = await _attendanceService.GetAttendancesAsync(filter);
            return Ok(result);
        }
        catch (ConflictException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Получение статистики посещаемости студента
    /// </summary>
    [HttpGet("stats/student/{studentId}")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetStudentAttendanceStats(
        Guid studentId,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var result = await _attendanceService.GetStudentAttendanceStatsAsync(studentId, fromDate, toDate);
        return Ok(result);
    }

    /// <summary>
    /// Получение отчета посещаемости для группы
    /// </summary>
    [HttpGet("report/group/{groupId}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> GetGroupAttendanceReport(
        Guid groupId,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var actualFromDate = fromDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30)); // Последние 30 дней
        var actualToDate = toDate ?? DateOnly.FromDateTime(DateTime.Today);

        if (actualFromDate > actualToDate)
        {
            return BadRequest(new { 
                error = "Дата начала не может быть больше даты окончания",
                fromDate = actualFromDate,
                toDate = actualToDate
            });
        }

        var result = await _attendanceService.GetGroupAttendanceReportAsync(groupId, actualFromDate, actualToDate);
        return Ok(result);
    }

    /// <summary>
    /// Экспорт отчета посещаемости в Excel
    /// </summary>
    [HttpPost("export")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> ExportAttendanceReport([FromBody] AttendanceExportFilterModel filter)
    {
        try
        {
            var excelBytes = await _attendanceService.ExportAttendanceReportAsync(filter);
            var fileName = $"attendance_report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (ConflictException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}