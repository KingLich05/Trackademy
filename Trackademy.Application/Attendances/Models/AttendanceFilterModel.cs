using Trackademy.Domain.Enums;

namespace Trackademy.Application.Attendances.Models;

public class AttendanceFilterModel
{
    public Guid OrganizationId { get; set; }
    public string? StudentSearch { get; set; } // Поиск по FullName и Login
    public Guid? GroupId { get; set; }
    public Guid? SubjectId { get; set; }
    public AttendanceStatus? Status { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    
    // Пагинация
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}