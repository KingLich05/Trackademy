using Trackademy.Domain.Enums;

namespace Trackademy.Application.PaymentServices.Models;

public class PaymentStatsDto
{
    public int TotalPayments { get; set; }
    public int PendingPayments { get; set; }
    public int PaidPayments { get; set; }
    public int OverduePayments { get; set; }
    public int CancelledPayments { get; set; }
    public int RefundedPayments { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal OverdueAmount { get; set; }
}

public class PaymentFilterRequest
{
    public Guid OrganizationId { get; set; }
    public Guid? GroupId { get; set; }
    public PaymentStatus? Status { get; set; }
    public PaymentType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}