using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.GroupServices.Models;

public class GetGroupsRequest : PagedRequest
{
    public Guid OrganizationId { get; set; }
    
    /// <summary>
    /// Фильтр по предмету (необязательный)
    /// </summary>
    public Guid? SubjectId { get; set; }
    
    /// <summary>
    /// Поиск по названию группы или имени студента (необязательный)
    /// </summary>
    public string? Search { get; set; }
}