namespace Trackademy.Application.Services.Models;

public class ExportGroupsRequest
{
    public Guid OrganizationId { get; set; }
    public Guid? GroupId { get; set; }
    public bool IncludePayments { get; set; } = false;
}
