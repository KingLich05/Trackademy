using FluentValidation;
using Trackademy.Application.AssignmentServices.Models;

namespace Trackademy.Application.AssignmentServices.Validators;

public class AssignmentAddModelValidator : AbstractValidator<AssignmentAddModel>
{
    public AssignmentAddModelValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание задания обязательно")
            .MaximumLength(1000).WithMessage("Описание задания не может превышать 1000 символов");

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("ID группы обязателен");

        RuleFor(x => x.AssignedDate)
            .NotEmpty().WithMessage("Дата назначения обязательна");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Дата выполнения обязательна")
            .GreaterThan(x => x.AssignedDate).WithMessage("Дата выполнения должна быть позже даты назначения");
    }
}

public class AssignmentUpdateModelValidator : AbstractValidator<AssignmentUpdateModel>
{
    public AssignmentUpdateModelValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание задания обязательно")
            .MaximumLength(1000).WithMessage("Описание задания не может превышать 1000 символов");

        RuleFor(x => x.AssignedDate)
            .NotEmpty().WithMessage("Дата назначения обязательна");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Дата выполнения обязательна")
            .GreaterThan(x => x.AssignedDate).WithMessage("Дата выполнения должна быть позже даты назначения");
    }
}