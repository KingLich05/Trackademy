using Trackademy.Domain.Enums;

namespace Trackademy.Application.Users.Models;

public class UserDto
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string? ParentPhone { get; set; }
    public DateTime? Birthday { get; set; }
    public List<GroupMinimalViewModel> Groups { get; set; }
    public RoleEnum Role { get; set; }
}