using System.ComponentModel.DataAnnotations;

namespace Trackademy.Application.PaymentServices.Models;

public class UpdateDiscountRequest
{
    /// <summary>
    /// Процент скидки (от 0 до 100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Процент скидки должен быть от 0 до 100")]
    public decimal DiscountPercentage { get; set; }
    
    /// <summary>
    /// Причина скидки (опционально)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Причина скидки не должна превышать 200 символов")]
    public string? DiscountReason { get; set; }
}
