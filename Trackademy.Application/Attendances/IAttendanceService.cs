using Trackademy.Application.Attendances.Models;
using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.Attendances;

public interface IAttendanceService
{
    /// <summary>
    /// Массовая отметка посещаемости для урока
    /// </summary>
    Task<bool> MarkAttendancesAsync(AttendanceBulkCreateModel model);
    
    /// <summary>
    /// Обновление посещаемости студента для урока
    /// </summary>
    Task<AttendanceDto?> UpdateAttendanceAsync(AttendanceUpdateModel model);
    
    /// <summary>
    /// Получение посещаемости по ID
    /// </summary>
    Task<AttendanceDto?> GetAttendanceByIdAsync(Guid id);
    
    /// <summary>
    /// Получение списка посещаемости с фильтрацией
    /// </summary>
    Task<PagedResult<AttendanceDto>> GetAttendancesAsync(AttendanceFilterModel filter, Guid userId, string userRole);
    
    /// <summary>
    /// Получение статистики посещаемости студента
    /// </summary>
    Task<AttendanceStatsDto> GetStudentAttendanceStatsAsync(Guid studentId, DateOnly? fromDate = null, DateOnly? toDate = null);
    
    /// <summary>
    /// Получение отчета посещаемости для группы
    /// </summary>
    Task<List<AttendanceReportDto>> GetGroupAttendanceReportAsync(Guid groupId, DateOnly fromDate, DateOnly toDate);
    
    /// <summary>
    /// Экспорт отчета посещаемости в Excel
    /// </summary>
    Task<byte[]> ExportAttendanceReportAsync(AttendanceExportFilterModel filter);
}