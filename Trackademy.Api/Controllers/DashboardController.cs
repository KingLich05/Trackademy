using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Dashboard;
using Trackademy.Application.Dashboard.Models;
using Trackademy.Api.Authorization;
using Trackademy.Domain.Enums;
using System.Security.Claims;

namespace Trackademy.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[RoleAuthorization(RoleEnum.Administrator)]

public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// 📊 Получить краткую сводку дашборда - основные метрики
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary([FromQuery] DashboardFilterDto? filter = null)
    {
        try
        {
            var summary = await _dashboardService.GetDashboardSummaryAsync(filter);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return BadRequest($"Ошибка получения краткой сводки: {ex.Message}");
        }
    }

    /// <summary>
    /// 📈 Получить детальный отчет дашборда - расширенная информация
    /// </summary>
    [HttpGet("detailed")]
    public async Task<ActionResult<DashboardDetailedDto>> GetDashboardDetailed([FromQuery] DashboardFilterDto? filter = null)
    {
        try
        {
            var detailed = await _dashboardService.GetDashboardDetailedAsync(filter);
            return Ok(detailed);
        }
        catch (Exception ex)
        {
            return BadRequest($"Ошибка получения детального отчета: {ex.Message}");
        }
    }

    /// <summary>
    /// 👨‍🏫 Получить дашборд для преподавателя
    /// </summary>
    [HttpGet("teacher")]
    [AllowAnonymous]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<TeacherDashboardDto>> GetTeacherDashboard()
    {
        try
        {
            var teacherId = GetCurrentUserId();
            var dashboard = await _dashboardService.GetTeacherDashboardAsync(teacherId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            return BadRequest($"Ошибка получения дашборда преподавателя: {ex.Message}");
        }
    }

    /// <summary>
    /// 👨‍🎓 Получить дашборд для студента
    /// </summary>
    [HttpGet("student")]
    [AllowAnonymous]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard()
    {
        try
        {
            var studentId = GetCurrentUserId();
            var dashboard = await _dashboardService.GetStudentDashboardAsync(studentId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            return BadRequest($"Ошибка получения дашборда студента: {ex.Message}");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}