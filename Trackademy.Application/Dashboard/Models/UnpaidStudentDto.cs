using Trackademy.Domain.Enums;

namespace Trackademy.Application.Dashboard.Models;

/// <summary>
/// Студент, который не оплатил обучение
/// </summary>
public class UnpaidStudentDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>
    /// Название группы
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// Сумма задолженности
    /// </summary>
    public decimal DebtAmount { get; set; }
    
    /// <summary>
    /// Количество дней просрочки
    /// </summary>
    public int DaysOverdue { get; set; }
    
    /// <summary>
    /// Статус платежа
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; }
    
    /// <summary>
    /// Последняя дата платежа
    /// </summary>
    public DateTime? LastPaymentDate { get; set; }
}