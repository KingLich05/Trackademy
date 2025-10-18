using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Dashboard;
using Trackademy.Application.Dashboard.Models;

namespace Trackademy.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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
}