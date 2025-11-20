using FluentValidation;
using Trackademy.Application.SubjectServices.Models;

namespace Trackademy.Application.SubjectServices.Validators;

public class SubjectAddModelValidator : AbstractValidator<SubjectAddModel>
{
    public SubjectAddModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название предмета обязательно")
            .MaximumLength(200).WithMessage("Название предмета не может превышать 200 символов");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не может превышать 1000 символов");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Цена не может быть отрицательной");

        RuleFor(x => x.PaymentType)
            .IsInEnum()
            .WithMessage("Некорректный тип оплаты");

        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("ID организации обязателен");
    }
}

public class SubjectUpdateModelValidator : AbstractValidator<SubjectUpdateModel>
{
    public SubjectUpdateModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название предмета обязательно")
            .MaximumLength(200).WithMessage("Название предмета не может превышать 200 символов");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не может превышать 1000 символов");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Цена не может быть отрицательной");

        RuleFor(x => x.PaymentType)
            .IsInEnum()
            .WithMessage("Некорректный тип оплаты");
    }
}
