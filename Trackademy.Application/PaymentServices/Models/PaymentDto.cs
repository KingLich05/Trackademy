using Trackademy.Domain.Enums;

namespace Trackademy.Application.PaymentServices.Models;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string PaymentPeriod { get; set; } = string.Empty;
    public PaymentType Type { get; set; }
    public string TypeName => Type == PaymentType.Monthly ? "Ежемесячный" : "Единоразовый";
    public decimal OriginalAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal Amount { get; set; }
    public string? DiscountReason { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public PaymentStatus Status { get; set; }
    public string StatusName => Status switch
    {
        PaymentStatus.Pending => "Ожидает оплаты",
        PaymentStatus.Paid => "Оплачен",
        PaymentStatus.Overdue => "Просрочен",
        PaymentStatus.Cancelled => "Отменен",
        PaymentStatus.Refunded => "Возврат",
        _ => "Неизвестно"
    };
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public bool IsOverdue => Status == PaymentStatus.Pending && DateOnly.FromDateTime(DateTime.UtcNow) > PeriodEnd;
    public int DaysUntilEnd => (PeriodEnd.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber);
}