using Trackademy.Domain.Enums;
using Trackademy.Domain.Common;

namespace Trackademy.Domain.Users;

public class Payment : Entity
{
    public Guid StudentId { get; set; }
    public Guid GroupId { get; set; }
    public string PaymentPeriod { get; set; } = string.Empty;
    public PaymentType Type { get; set; }
    public decimal OriginalAmount { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; } = 0;
    public decimal Amount { get; set; }
    public string? DiscountReason { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    
    public User Student { get; set; } = null!;
    public Groups Group { get; set; } = null!;
}