using Trackademy.Domain.Enums;

namespace Trackademy.Application.Users.Models;

public class UserCreationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public UserCreatedDto? User { get; set; }
}

public class UserCreatedDto
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public required string Login { get; set; }
    public required string Role { get; set; }
}