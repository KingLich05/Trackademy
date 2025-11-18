using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.AssignmentServices.Models;

public class GetAssignmentsRequest : PagedRequest
{
    public Guid OrganizationId { get; set; }
    public Guid? GroupId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}