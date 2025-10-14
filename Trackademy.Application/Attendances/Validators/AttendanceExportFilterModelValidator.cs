using FluentValidation;
using Trackademy.Application.Attendances.Models;

namespace Trackademy.Application.Attendances.Validators;

public class AttendanceExportFilterModelValidator : AbstractValidator<AttendanceExportFilterModel>
{
    public AttendanceExportFilterModelValidator()
    {
        RuleFor(x => x.FromDate)
            .NotEmpty().WithMessage("Дата начала обязательна для экспорта");

        RuleFor(x => x.ToDate)
            .NotEmpty().WithMessage("Дата окончания обязательна для экспорта")
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("Дата окончания должна быть позже или равна дате начала");

        RuleFor(x => x.ToDate)
            .Must((model, toDate) => toDate.DayNumber - model.FromDate.DayNumber <= 365)
            .WithMessage("Период экспорта не может превышать 365 дней");
    }
}