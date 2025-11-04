using Trackademy.Domain.Enums;

namespace Trackademy.Application.PaymentServices.Models;

public class PaymentCreateModel
{
    public Guid StudentId { get; set; }
    public Guid GroupId { get; set; }
    public string? Description { get; set; }
    public PaymentType Type { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public string? DiscountReason { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime DueDate { get; set; }
}