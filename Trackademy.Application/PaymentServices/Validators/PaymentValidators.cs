using FluentValidation;
using Trackademy.Application.PaymentServices.Models;

namespace Trackademy.Application.PaymentServices.Validators;

public class PaymentCancelModelValidator : AbstractValidator<PaymentCancelModel>
{
    public PaymentCancelModelValidator()
    {
        RuleFor(x => x.CancelReason)
            .NotEmpty()
            .WithMessage("Причина отмены обязательна")
            .MaximumLength(500)
            .WithMessage("Причина отмены не может превышать 500 символов");
    }
}

public class PaymentRefundModelValidator : AbstractValidator<PaymentRefundModel>
{
    public PaymentRefundModelValidator()
    {
        RuleFor(x => x.RefundReason)
            .NotEmpty()
            .WithMessage("Причина возврата обязательна")
            .MaximumLength(500)
            .WithMessage("Причина возврата не может превышать 500 символов");
    }
}