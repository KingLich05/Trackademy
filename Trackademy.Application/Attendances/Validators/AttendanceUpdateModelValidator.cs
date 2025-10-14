using FluentValidation;
using Trackademy.Application.Attendances.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Attendances.Validators;

public class AttendanceUpdateModelValidator : AbstractValidator<AttendanceUpdateModel>
{
    public AttendanceUpdateModelValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("ID студента обязателен");

        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("ID урока обязателен");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Статус посещаемости должен быть корректным")
            .Must(BeValidAttendanceStatus).WithMessage("Неподдерживаемый статус посещаемости");
    }

    private static bool BeValidAttendanceStatus(AttendanceStatus status)
    {
        return Enum.IsDefined(typeof(AttendanceStatus), status);
    }
}