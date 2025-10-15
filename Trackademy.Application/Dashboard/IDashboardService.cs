using Trackademy.Application.Dashboard.Models;

namespace Trackademy.Application.Dashboard;

/// <summary>
/// Интерфейс для работы с дашбордом и аналитикой
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Получить полную статистику дашборда
    /// </summary>
    Task<DashboardOverviewDto> GetDashboardOverviewAsync(DashboardFilterDto? filter = null);
    
    /// <summary>
    /// Получить статистику по студентам
    /// </summary>
    Task<StudentStatsDto> GetStudentStatsAsync(DashboardFilterDto? filter = null);
    
    /// <summary>
    /// Получить статистику по группам
    /// </summary>
    Task<GroupStatsDto> GetGroupStatsAsync(DashboardFilterDto? filter = null);
    
    /// <summary>
    /// Получить статистику по урокам
    /// </summary>
    Task<LessonStatsDto> GetLessonStatsAsync(DashboardFilterDto? filter = null);
    
    /// <summary>
    /// Получить статистику по посещаемости
    /// </summary>
    Task<AttendanceStatsDto> GetAttendanceStatsAsync(DashboardFilterDto? filter = null);
    
    /// <summary>
    /// Получить группы с низкой успеваемостью
    /// </summary>
    Task<List<LowPerformanceGroupDto>> GetLowPerformanceGroupsAsync(DashboardFilterDto? filter = null);
    
    /// <summary>
    /// Получить список студентов с задолженностью
    /// </summary>
    Task<List<UnpaidStudentDto>> GetUnpaidStudentsAsync(DashboardFilterDto? filter = null);
    
    /// <summary>
    /// Получить список студентов на пробных уроках
    /// </summary>
    Task<List<TrialStudentDto>> GetTrialStudentsAsync(DashboardFilterDto? filter = null);
    
    /// <summary>
    /// Получить топ преподавателей
    /// </summary>
    Task<List<TopTeacherDto>> GetTopTeachersAsync(DashboardFilterDto? filter = null, int limit = 10);
    
    /// <summary>
    /// Получить информацию о последних обновлениях расписания
    /// </summary>
    Task<LatestScheduleUpdateDto?> GetLatestScheduleUpdateAsync(DashboardFilterDto? filter = null);
    
    /// <summary>
    /// Получить посещаемость по конкретной группе
    /// </summary>
    Task<GroupAttendanceDto> GetGroupAttendanceAsync(Guid groupId, DashboardFilterDto? filter = null);
}