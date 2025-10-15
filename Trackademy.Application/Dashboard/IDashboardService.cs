using Trackademy.Application.Dashboard.Models;

namespace Trackademy.Application.Dashboard;

/// <summary>
/// Интерфейс для работы с дашбордом и аналитикой
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// 📊 Получить краткую сводку дашборда - основные метрики
    /// </summary>
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(DashboardFilterDto? filter = null);
    
    /// <summary>
    /// 📈 Получить детальный отчет дашборда - расширенная информация
    /// </summary>
    Task<DashboardDetailedDto> GetDashboardDetailedAsync(DashboardFilterDto? filter = null);
}