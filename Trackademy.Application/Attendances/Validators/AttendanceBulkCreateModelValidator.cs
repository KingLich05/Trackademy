using FluentValidation;
using Trackademy.Application.Attendances.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Attendances.Validators;

public class AttendanceBulkCreateModelValidator : AbstractValidator<AttendanceBulkCreateModel>
{
    public AttendanceBulkCreateModelValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("ID урока обязателен");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Дата обязательна");

        RuleFor(x => x.Attendances)
            .NotNull().WithMessage("Список посещаемости обязателен")
            .Must(list => list.Count > 0).WithMessage("Список посещаемости не может быть пустым");

        RuleForEach(x => x.Attendances).SetValidator(new AttendanceRecordModelValidator());
    }
}

public class AttendanceRecordModelValidator : AbstractValidator<AttendanceRecordModel>
{
    public AttendanceRecordModelValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("ID студента обязателен");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Статус посещаемости должен быть корректным")
            .Must(BeValidAttendanceStatus).WithMessage("Неподдерживаемый статус посещаемости");
    }

    private static bool BeValidAttendanceStatus(AttendanceStatus status)
    {
        return Enum.IsDefined(typeof(AttendanceStatus), status);
    }
}