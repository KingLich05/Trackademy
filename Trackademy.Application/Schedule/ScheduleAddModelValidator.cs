using FluentValidation;
using Trackademy.Application.Schedule.Model;

namespace Trackademy.Application.Schedule;

public class ScheduleAddModelValidator : AbstractValidator<ScheduleAddModel>
{
    public ScheduleAddModelValidator()
    {
        RuleFor(x => x.DaysOfWeek)
            .NotNull().WithMessage("DaysOfWeek is required")
            .Must(d => d != null && d.Length > 0).WithMessage("At least one day of week must be provided")
            .Must(d => d == null || d.All(day => day >= 1 && day <= 7))
            .WithMessage("DaysOfWeek must be between 1 (Monday) and 7 (Sunday)")
            .Must(d => d == null || d.Distinct().Count() == d.Length)
            .WithMessage("DaysOfWeek must not contain duplicates");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("StartTime is required")
            .Must(BeValidTime).WithMessage("StartTime must be a valid time");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required")
            .Must(BeValidTime).WithMessage("EndTime must be a valid time")
            .GreaterThan(x => x.StartTime)
            .WithMessage("EndTime must be later than StartTime");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("EffectiveFrom date is required")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("EffectiveFrom must be today or in the future");

        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo must be equal or later than EffectiveFrom");

        RuleFor(x => x.GroupId)
            .NotEqual(Guid.Empty).WithMessage("GroupId is required");

        RuleFor(x => x.TeacherId)
            .NotEqual(Guid.Empty).WithMessage("TeacherId is required");

        RuleFor(x => x.RoomId)
            .NotEqual(Guid.Empty).WithMessage("RoomId is required");

        RuleFor(x => x.OrganizationId)
            .NotEqual(Guid.Empty).WithMessage("OrganizationId is required");
    }

    /// <summary>
    /// Проверка, что время является валидным (в пределах суток)
    /// </summary>
    private static bool BeValidTime(TimeSpan time)
    {
        return time >= TimeSpan.Zero && time < TimeSpan.FromDays(1);
    }
}