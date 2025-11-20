using Trackademy.Domain.Enums;

namespace Trackademy.Application.SubjectServices.Models;

public class SubjectAddModel
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public int Price { get; set; }
    public PaymentType PaymentType { get; set; } = PaymentType.Monthly;
    public Guid OrganizationId { get; set; }
}