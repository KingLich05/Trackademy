using Trackademy.Domain.Enums;

namespace Trackademy.Application.PaymentServices.Models;

public class BulkPaymentCreateModel
{
    public Guid GroupId { get; set; }
    public required string PaymentPeriod { get; set; }
    public PaymentType Type { get; set; } = PaymentType.Monthly;
    public decimal OriginalAmount { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public string? DiscountReason { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public bool ApplyToAllStudents { get; set; } = true;
    public List<Guid>? SpecificStudentIds { get; set; }
}