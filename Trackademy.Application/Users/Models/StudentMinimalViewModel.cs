namespace Trackademy.Application.Users.Models;

public class StudentMinimalViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string? PhotoPath { get; set; }
}