using Trackademy.Domain.Enums;

namespace Trackademy.Application.PaymentServices.Models;

public class GroupedPaymentDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    
    // Информация о последнем платеже на уровне студента
    public Guid LastPaymentId { get; set; }
    public decimal LastPaymentAmount { get; set; }
    public PaymentStatus LastPaymentStatus { get; set; }
    public string LastPaymentStatusName { get; set; } = string.Empty;
    public PaymentType LastPaymentType { get; set; }
    public string LastPaymentTypeName { get; set; } = string.Empty;
    public string LastPaymentPeriod { get; set; } = string.Empty;
    public DateTime LastPaymentCreatedAt { get; set; }
    public DateTime? LastPaymentPaidAt { get; set; }
    public DateOnly LastPaymentPeriodStart { get; set; }
    public DateOnly LastPaymentPeriodEnd { get; set; }
    public string? LastPaymentDiscountReason { get; set; }
    public decimal LastPaymentOriginalAmount { get; set; }
    public DiscountType LastPaymentDiscountType { get; set; }
    public decimal LastPaymentDiscountValue { get; set; }
    
    // Все платежи студента
    public List<PaymentDto> Payments { get; set; } = new();
}