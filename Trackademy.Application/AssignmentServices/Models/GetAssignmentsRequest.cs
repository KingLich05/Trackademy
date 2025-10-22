using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.AssignmentServices.Models;

public class GetAssignmentsRequest : PagedRequest
{
    public Guid OrganizationId { get; set; }
    public Guid? GroupId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}