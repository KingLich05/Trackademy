using Trackademy.Domain.Enums;

namespace Trackademy.Application.SubjectServices.Models;

public class SubjectUpdateModel
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public int Price { get; set; }
    public PaymentType PaymentType { get; set; }
}