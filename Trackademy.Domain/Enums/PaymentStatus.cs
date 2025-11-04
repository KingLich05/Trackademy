namespace Trackademy.Domain.Enums;

public enum PaymentStatus
{
    /// <summary>
    /// Ожидает оплаты
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// Оплачен
    /// </summary>
    Paid = 2,
    
    /// <summary>
    /// Просрочен (автоматически после DueDate)
    /// </summary>
    Overdue = 3,
    
    /// <summary>
    /// Отменен (студент отчислился/покинул группу)
    /// </summary>
    Cancelled = 4,
    
    /// <summary>
    /// Возврат средств по оплаченному платежу
    /// </summary>
    Refunded = 5
}