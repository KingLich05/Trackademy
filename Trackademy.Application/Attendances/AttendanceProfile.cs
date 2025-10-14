using AutoMapper;
using Trackademy.Application.Attendances.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Attendances;

public class AttendanceProfile : Profile
{
    public AttendanceProfile()
    {
        CreateMap<Attendance, AttendanceDto>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
            .ForMember(dest => dest.StudentLogin, opt => opt.MapFrom(src => src.Student.Login))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => GetStatusName(src.Status)))
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Lesson.Group.Subject.Name))
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Lesson.Group.Name))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Lesson.Teacher.FullName))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Lesson.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Lesson.EndTime));
    }

    private static string GetStatusName(AttendanceStatus status)
    {
        return status switch
        {
            AttendanceStatus.Attend => "Присутствовал",
            AttendanceStatus.NotAttend => "Отсутствовал",
            AttendanceStatus.Late => "Опоздал",
            AttendanceStatus.SpecialReason => "Уважительная причина",
            _ => "Неизвестно"
        };
    }
}