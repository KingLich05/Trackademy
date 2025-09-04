namespace Trackademy.Application.Users.Models;

public class LogRequest
{
    public string Email { get; set; }
    
    public string Password { get; set; }

    public Guid OrganizationId { get; set; }
}