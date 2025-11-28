using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Application.Services;
using Trackademy.Application.Services.Models;
using Trackademy.Application.Shared.Exception;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IExcelExportService _excelExportService;

    public ExportController(IExcelExportService excelExportService)
    {
        _excelExportService = excelExportService;
    }

    /// <summary>
    /// Экспорт всех пользователей организации (студенты и преподаватели)
    /// </summary>
    [HttpPost("users")]
    [Authorize(Roles = "Administrator,Owner")]
    public async Task<IActionResult> ExportUsers([FromBody] ExportUsersRequest request)
    {
        try
        {
            var excelBytes = await _excelExportService.ExportUsersAsync(request.OrganizationId);
            var fileName = $"users_export_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
            
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (ConflictException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Экспорт групп с информацией о студентах
    /// </summary>
    [HttpPost("groups")]
    [Authorize(Roles = "Administrator,Owner")]
    public async Task<IActionResult> ExportGroups([FromBody] ExportGroupsRequest request)
    {
        try
        {
            var excelBytes = await _excelExportService.ExportGroupsAsync(request);
            var fileName = $"groups_export_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
            
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (ConflictException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Экспорт платежей с группировкой
    /// Создает Excel файл с тремя видами представления:
    /// 1. Все платежи (один список)
    /// 2. По статусам (разделы на одном листе)
    /// 3. По группам (каждая группа = отдельный лист)
    /// </summary>
    [HttpPost("payments")]
    [Authorize(Roles = "Administrator,Owner")]
    public async Task<IActionResult> ExportPayments([FromBody] ExportPaymentsRequest request)
    {
        try
        {
            var excelBytes = await _excelExportService.ExportPaymentsAsync(request);
            var fileName = $"payments_export_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
            
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (ConflictException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
