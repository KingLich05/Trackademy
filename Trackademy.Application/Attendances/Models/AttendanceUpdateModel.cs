using Trackademy.Domain.Enums;

namespace Trackademy.Application.Attendances.Models;

public class AttendanceUpdateModel
{
    public Guid StudentId { get; set; }
    public Guid LessonId { get; set; }
    public AttendanceStatus Status { get; set; }
}