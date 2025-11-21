using Trackademy.Domain.Common;
using Trackademy.Domain.Enums;

namespace Trackademy.Domain.Users;

/// <summary>
/// Связующая таблица для отношения многие-ко-многим между Group и Student
/// с дополнительными данными о скидке студента в конкретной группе
/// </summary>
public class GroupStudent : Entity
{
    public Guid GroupId { get; set; }
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Тип скидки для студента в этой группе
    /// </summary>
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    
    /// <summary>
    /// Значение скидки (процент 0-100 или фиксированная сумма)
    /// </summary>
    public decimal DiscountValue { get; set; } = 0;
    
    /// <summary>
    /// Причина предоставления скидки
    /// </summary>
    public string? DiscountReason { get; set; }
    
    /// <summary>
    /// Дата добавления студента в группу
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Флаг заморозки студента в группе
    /// </summary>
    public bool IsFrozen { get; set; } = false;
    
    /// <summary>
    /// Дата начала заморозки
    /// </summary>
    public DateOnly? FrozenFrom { get; set; }
    
    /// <summary>
    /// Дата окончания заморозки
    /// </summary>
    public DateOnly? FrozenTo { get; set; }
    
    /// <summary>
    /// Причина заморозки
    /// </summary>
    public string? FreezeReason { get; set; }
    
    // Navigation properties
    public Groups Group { get; set; } = null!;
    public User Student { get; set; } = null!;
}
