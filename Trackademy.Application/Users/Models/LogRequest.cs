namespace Trackademy.Application.Users.Models;

public class LogRequest
{
    public string Login { get; set; }
    
    public string Password { get; set; }

    public Guid OrganizationId { get; set; }
}