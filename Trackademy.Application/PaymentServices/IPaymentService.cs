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
    /// Получение платежей, сгруппированных по студентам
    /// </summary>
    Task<GroupedPaymentResult> GetGroupedPaymentsAsync(PaymentFilterRequest request);
    
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
    /// Обновить скидку платежа
    /// </summary>
    Task<bool> UpdateDiscountAsync(Guid paymentId, UpdateDiscountRequest request);
    
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
    
    /// <summary>
    /// Создание платежей для списка студентов при добавлении в группу (batch операция)
    /// </summary>
    Task CreatePaymentsForStudentsAsync(List<Guid> studentIds, Guid groupId, DiscountType discountType = DiscountType.Percentage, decimal discountValue = 0, string? discountReason = null);
    
    /// <summary>
    /// Отмена всех неоплаченных платежей студента в группе (при удалении из группы)
    /// </summary>
    Task CancelStudentPaymentsInGroupAsync(Guid studentId, Guid groupId, string cancelReason);
    
    /// <summary>
    /// Создание ежемесячных платежей для всех активных студентов (вызывается фоновой службой)
    /// </summary>
    Task CreateMonthlyPaymentsAsync();
    
    /// <summary>
    /// Создание платежей для студентов с истекшей оплатой (вызывается фоновой службой каждые 6 часов)
    /// </summary>
    Task CreatePendingPaymentsForUnpaidStudentsAsync();
}