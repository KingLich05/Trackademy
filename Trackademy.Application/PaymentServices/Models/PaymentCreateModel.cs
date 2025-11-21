using Trackademy.Domain.Enums;

namespace Trackademy.Application.PaymentServices.Models;

public class PaymentCreateModel
{
    public Guid StudentId { get; set; }
    public Guid GroupId { get; set; }
    public required string PaymentPeriod { get; set; }
    public PaymentType Type { get; set; }
    public decimal OriginalAmount { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; } = 0;
    public string? DiscountReason { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}