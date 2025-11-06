using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.PaymentServices.Models;

public class GroupedPaymentResult
{
    public List<GroupedPaymentDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}