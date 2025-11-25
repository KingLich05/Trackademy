using Trackademy.Application.Attendances.Models;
using Trackademy.Application.Services.Models;

namespace Trackademy.Application.Services;

public interface IExcelExportService
{
    /// <summary>
    /// Экспорт отчета посещаемости в Excel
    /// </summary>
    Task<byte[]> ExportAttendanceReportAsync(List<AttendanceDto> attendances, AttendanceExportFilterModel filter);
    
    /// <summary>
    /// Экспорт всех пользователей организации (студенты и преподаватели)
    /// </summary>
    Task<byte[]> ExportUsersAsync(Guid organizationId);
}