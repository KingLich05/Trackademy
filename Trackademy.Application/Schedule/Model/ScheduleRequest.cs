using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.Schedule.Model;

public class ScheduleRequest : RequestIdOrganization
{
    public Guid? GroupId { get; set; }

    public Guid? TeacherId { get; set; }

    public Guid? RoomId { get; set; }

    public Guid? SubjectId { get; set; }
}