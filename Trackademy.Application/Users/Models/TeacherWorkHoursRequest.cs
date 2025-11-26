namespace Trackademy.Application.Users.Models;

public class TeacherWorkHoursRequest
{
    public Guid OrganizationId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
}
