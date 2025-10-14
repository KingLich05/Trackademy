using Trackademy.Domain.Enums;

namespace Trackademy.Application.Attendances.Models;

public class AttendanceExportFilterModel
{
    public Guid? OrganizationId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? StudentId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public AttendanceStatus? Status { get; set; }
}