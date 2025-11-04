namespace Trackademy.Application.PaymentServices.Models;

public class PaymentMarkAsPaidModel
{
    public DateTime? PaidAt { get; set; } = DateTime.UtcNow;
}