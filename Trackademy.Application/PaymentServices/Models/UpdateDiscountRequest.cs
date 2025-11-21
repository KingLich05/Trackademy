using System.ComponentModel.DataAnnotations;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.PaymentServices.Models;

public class UpdateDiscountRequest
{
    /// <summary>
    /// Тип скидки: Percentage (1) - проценты, FixedAmount (2) - фиксированная сумма
    /// </summary>
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    
    /// <summary>
    /// Значение скидки:
    /// - Если DiscountType = Percentage: от 0 до 100 (процент)
    /// - Если DiscountType = FixedAmount: сумма скидки в валюте
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Значение скидки должно быть больше или равно 0")]
    public decimal DiscountValue { get; set; }
    
    /// <summary>
    /// Причина скидки (опционально)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Причина скидки не должна превышать 200 символов")]
    public string? DiscountReason { get; set; }
}
