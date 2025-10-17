using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.GroupServices.Models;

public class GetGroupsRequest : PagedRequest
{
    public Guid OrganizationId { get; set; }
}