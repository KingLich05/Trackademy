using Trackademy.Domain.Enums;

namespace Trackademy.Application.Users.Models;

public class GetUserRequest
{
    public string? search {get;set;}
    
    public List<Guid>? GroupIds { get; set; }
    
    public List<RoleEnum>? RoleIds { get; set; }
}