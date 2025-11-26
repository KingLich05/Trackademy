using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.MaterialServices.Models;

public class GetMaterialsRequest : PagedRequest
{
    public Guid? GroupId { get; set; }
    public string? SearchTitle { get; set; }
    public Guid? OrganizationId { get; set; }
    public bool SortByDateDescending { get; set; } = true;
}
