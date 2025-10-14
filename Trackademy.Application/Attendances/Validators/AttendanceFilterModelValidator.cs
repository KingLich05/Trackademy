using FluentValidation;
using Trackademy.Application.Attendances.Models;

namespace Trackademy.Application.Attendances.Validators;

public class AttendanceFilterModelValidator : AbstractValidator<AttendanceFilterModel>
{
    public AttendanceFilterModelValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Номер страницы должен быть больше 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Размер страницы должен быть больше 0")
            .LessThanOrEqualTo(100).WithMessage("Размер страницы не может превышать 100");

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(x => x.ToDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("Дата начала должна быть раньше или равна дате окончания");

        RuleFor(x => x.StudentSearch)
            .MaximumLength(100).WithMessage("Поисковый запрос не может превышать 100 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.StudentSearch));
    }
}