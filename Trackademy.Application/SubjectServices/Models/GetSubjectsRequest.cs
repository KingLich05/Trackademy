using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.SubjectServices.Models;

public class GetSubjectsRequest : PagedRequest
{
    public Guid OrganizationId { get; set; }
}