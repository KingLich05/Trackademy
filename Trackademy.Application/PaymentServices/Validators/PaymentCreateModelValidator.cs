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

        RuleFor(x => x.PaymentPeriod)
            .NotEmpty()
            .WithMessage("Период оплаты обязателен")
            .MaximumLength(500)
            .WithMessage("Период оплаты не может превышать 500 символов");

        RuleFor(x => x.OriginalAmount)
            .GreaterThan(0)
            .WithMessage("Сумма должна быть больше 0");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100)
            .WithMessage("Скидка должна быть от 0 до 100%");

        RuleFor(x => x.PeriodStart)
            .LessThan(x => x.PeriodEnd)
            .WithMessage("Дата начала периода должна быть раньше даты окончания");

        RuleFor(x => x.PeriodEnd)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Период окончания не может быть в прошлом");

        RuleFor(x => x.PeriodEnd)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddYears(2)))
            .WithMessage("Период окончания не может быть более чем через 2 года");

        RuleFor(x => x.DiscountReason)
            .MaximumLength(200)
            .WithMessage("Причина скидки не может превышать 200 символов")
            .When(x => !string.IsNullOrEmpty(x.DiscountReason));

        // Валидация что если есть скидка, то должна быть причина
        RuleFor(x => x.DiscountReason)
            .NotEmpty()
            .WithMessage("При наличии скидки необходимо указать причину")
            .When(x => x.DiscountPercentage > 0);
    }
}