using Trackademy.Domain.Common;

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
    /// Процент скидки для студента в этой группе (0-100)
    /// </summary>
    public decimal DiscountPercentage { get; set; } = 0;
    
    /// <summary>
    /// Причина предоставления скидки
    /// </summary>
    public string? DiscountReason { get; set; }
    
    /// <summary>
    /// Дата добавления студента в группу
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Groups Group { get; set; } = null!;
    public User Student { get; set; } = null!;
}
