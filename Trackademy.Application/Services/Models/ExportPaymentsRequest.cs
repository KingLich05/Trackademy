namespace Trackademy.Application.Services.Models;

public class ExportPaymentsRequest
{
    /// <summary>
    /// ID организации (обязательно)
    /// </summary>
    public Guid OrganizationId { get; set; }
    
    /// <summary>
    /// Фильтр по группе (опционально)
    /// </summary>
    public Guid? GroupId { get; set; }
    
    /// <summary>
    /// Фильтр по статусу платежа (опционально)
    /// </summary>
    public int? Status { get; set; }
    
    /// <summary>
    /// Фильтр по студенту (опционально)
    /// </summary>
    public Guid? StudentId { get; set; }
    
    /// <summary>
    /// Начало периода (опционально)
    /// </summary>
    public DateTime? PeriodFrom { get; set; }
    
    /// <summary>
    /// Конец периода (опционально)
    /// </summary>
    public DateTime? PeriodTo { get; set; }
}
