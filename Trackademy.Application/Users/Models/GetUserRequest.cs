using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Users.Models;

public class GetUserRequest : PagedRequest
{
    public string? search {get;set;}
    
    public List<Guid>? GroupIds { get; set; }
    
    public List<RoleEnum>? RoleIds { get; set; }
    
    /// <summary>
    /// Фильтр по флагу пробного урока (необязательный)
    /// null - все пользователи, true - только пробные, false - только не пробные
    /// </summary>
    public bool? IsTrial { get; set; }
    
    public Guid OrganizationId { get; set; }
}