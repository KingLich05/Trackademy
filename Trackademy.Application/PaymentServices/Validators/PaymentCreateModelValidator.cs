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

        RuleFor(x => x.DiscountValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Значение скидки должно быть больше или равно 0");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100)
            .WithMessage("Процент скидки должен быть от 0 до 100")
            .When(x => x.DiscountType == Domain.Enums.DiscountType.Percentage);

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(x => x.OriginalAmount)
            .WithMessage("Фиксированная скидка не может превышать исходную сумму")
            .When(x => x.DiscountType == Domain.Enums.DiscountType.FixedAmount);

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
            .When(x => x.DiscountValue > 0);
    }
}