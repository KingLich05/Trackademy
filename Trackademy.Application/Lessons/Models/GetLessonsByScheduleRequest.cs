using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.Lessons.Models;

public class GetLessonsByScheduleRequest : PagedRequest
{
    public Guid OrganizationId { get; set; }
    public Guid? ScheduleId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? SubjectId { get; set; }
}