using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Attendances;
using Trackademy.Application.Attendances.Models;

namespace Trackademy.Api.Controllers.Attendanc;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> MarkAttendancesBulk([FromBody] AttendanceBulkCreateModel model)
    {
        var result = await _attendanceService.MarkAttendancesAsync(model);
        return Ok(new { success = result });
    }

    /// <summary>
    /// Обновление статуса посещаемости для конкретного студента
    /// </summary>
    [HttpPut("update")]
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
    public async Task<IActionResult> GetAttendances([FromBody] AttendanceFilterModel filter)
    {
        var result = await _attendanceService.GetAttendancesAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Получение статистики посещаемости студента
    /// </summary>
    [HttpGet("stats/student/{studentId}")]
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
    public async Task<IActionResult> GetGroupAttendanceReport(
        Guid groupId,
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate)
    {
        var result = await _attendanceService.GetGroupAttendanceReportAsync(groupId, fromDate, toDate);
        return Ok(result);
    }

    /// <summary>
    /// Экспорт отчета посещаемости в Excel
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportAttendanceReport([FromBody] AttendanceExportFilterModel filter)
    {
        var excelBytes = await _attendanceService.ExportAttendanceReportAsync(filter);
        var fileName = $"attendance_report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Экспорт детального отчета посещаемости для группы в Excel
    /// </summary>
    [HttpGet("export/group/{groupId}")]
    public async Task<IActionResult> ExportGroupAttendanceReport(
        Guid groupId,
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate)
    {
        var groupReport = await _attendanceService.GetGroupAttendanceReportAsync(groupId, fromDate, toDate);
        
        // Получим название группы для файла - возьмем первый код группы или используем ID
        var groupName = groupId.ToString()[..8]; // Первые 8 символов ID как название
        
        var excelBytes = await _attendanceService.ExportGroupReportToExcelAsync(groupReport, groupName, fromDate, toDate);
        var fileName = $"group_{groupName}_attendance_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
        
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}