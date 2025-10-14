using Trackademy.Domain.Enums;

namespace Trackademy.Application.Attendances.Models;

public class AttendanceBulkCreateModel
{
    public Guid LessonId { get; set; }
    public DateOnly Date { get; set; }
    public List<AttendanceRecordModel> Attendances { get; set; } = new();
}

public class AttendanceRecordModel
{
    public Guid StudentId { get; set; }
    public AttendanceStatus Status { get; set; }
}