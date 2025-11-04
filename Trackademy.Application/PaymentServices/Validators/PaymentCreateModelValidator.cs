using FluentValidation;
using Trackademy.Application.PaymentServices.Models;

namespace Trackademy.Application.PaymentServices.Validators;

public class PaymentCreateModelValidator : AbstractValidator<PaymentCreateModel>
{
    public PaymentCreateModelValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty()
            .WithMessage("ID студента обязателен");

        RuleFor(x => x.GroupId)
            .NotEmpty()
            .WithMessage("ID группы обязателен");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Описание платежа обязательно")
            .MaximumLength(500)
            .WithMessage("Описание не может превышать 500 символов");

        RuleFor(x => x.OriginalAmount)
            .GreaterThan(0)
            .WithMessage("Сумма должна быть больше 0");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100)
            .WithMessage("Скидка должна быть от 0 до 100%");

        RuleFor(x => x.PeriodStart)
            .LessThan(x => x.PeriodEnd)
            .WithMessage("Дата начала периода должна быть раньше даты окончания");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Срок оплаты не может быть в прошлом");

        RuleFor(x => x.DiscountReason)
            .MaximumLength(200)
            .WithMessage("Причина скидки не может превышать 200 символов")
            .When(x => !string.IsNullOrEmpty(x.DiscountReason));
    }
}