namespace Trackademy.Application.Users.Models;

public class TeacherWorkHoursDto
{
    public Guid TeacherId { get; set; }
    public string FullName { get; set; }
    public int CompletedLessonsCount { get; set; }
}
