using Trackademy.Application.PaymentServices.Models;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.PaymentServices;

public interface IPaymentService
{
    /// <summary>
    /// Создание платежа администратором
    /// </summary>
    Task<Guid> CreatePaymentAsync(PaymentCreateModel model);
    
    /// <summary>
    /// Получение платежа по ID
    /// </summary>
    Task<PaymentDto?> GetPaymentByIdAsync(Guid id);
    
    /// <summary>
    /// Получение всех платежей студента
    /// </summary>
    Task<List<PaymentDto>> GetStudentPaymentsAsync(Guid studentId);
    
    /// <summary>
    /// Получение платежей группы
    /// </summary>
    Task<PagedResult<PaymentDto>> GetGroupPaymentsAsync(Guid groupId, int page = 1, int pageSize = 10);
    
    /// <summary>
    /// Получение всех платежей с расширенными фильтрами (для админов)
    /// </summary>
    Task<PagedResult<PaymentDto>> GetPaymentsWithFiltersAsync(PaymentFilterRequest request);
    
    /// <summary>
    /// Пометить платеж как оплаченный
    /// </summary>
    Task<bool> MarkPaymentAsPaidAsync(Guid paymentId, PaymentMarkAsPaidModel model);
    
    /// <summary>
    /// Отменить платеж (при отчислении студента)
    /// </summary>
    Task<bool> CancelPaymentAsync(Guid paymentId, PaymentCancelModel model);
    
    /// <summary>
    /// Сделать возврат по оплаченному платежу
    /// </summary>
    Task<bool> RefundPaymentAsync(Guid paymentId, PaymentRefundModel model);
    
    /// <summary>
    /// Автоматическое обновление просроченных платежей
    /// </summary>
    Task UpdateOverduePaymentsAsync();
    
    /// <summary>
    /// Получение платежей, требующих уведомления (за 3 дня до окончания периода)
    /// </summary>
    Task<List<PaymentDto>> GetPaymentsForNotificationAsync();

    /// <summary>
    /// Получение статистики платежей
    /// </summary>
    Task<PaymentStatsDto> GetPaymentStatsAsync(Guid organizationId, Guid? groupId = null, Guid? studentId = null);
}