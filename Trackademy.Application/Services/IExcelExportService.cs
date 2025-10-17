using Trackademy.Application.Attendances.Models;

namespace Trackademy.Application.Services;

public interface IExcelExportService
{
    /// <summary>
    /// Экспорт отчета посещаемости в Excel
    /// </summary>
    Task<byte[]> ExportAttendanceReportAsync(List<AttendanceDto> attendances, AttendanceExportFilterModel filter);
}