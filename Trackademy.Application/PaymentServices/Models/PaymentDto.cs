using Trackademy.Domain.Enums;

namespace Trackademy.Application.PaymentServices.Models;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PaymentType Type { get; set; }
    public string TypeName => Type == PaymentType.Monthly ? "Ежемесячный" : "Единоразовый";
    public decimal OriginalAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal Amount { get; set; }
    public string? DiscountReason { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime DueDate { get; set; }
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
    public bool IsOverdue => Status == PaymentStatus.Pending && DateTime.UtcNow.Date > DueDate.Date;
    public int DaysUntilDue => (DueDate.Date - DateTime.UtcNow.Date).Days;
}