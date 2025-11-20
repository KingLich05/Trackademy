using Trackademy.Domain.Common;
using Trackademy.Domain.Enums;

namespace Trackademy.Domain.Users;

public class Subject : BaseEntity
{
    public string? Description { get; set; }
    
    public int Price { get; set; }
    
    public PaymentType PaymentType { get; set; } = PaymentType.Monthly;
    
    public Guid OrganizationId { get; set; }
    
    // нав. поля.
    public List<Groups> Groups { get; set; }
    
    public Organization Organization { get; set; }
}