using AutoMapper;
using Trackademy.Application.RoomServices.Models;
using Trackademy.Application.Schedule.Model;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Lessons;

public class LessonProfile : Profile
{
    public LessonProfile()
    {
        CreateMap<Lesson, LessonViewModel>()
            .ForMember(d => d.Group, opt => opt.MapFrom(s => s.Group))
            .ForMember(d => d.Subject, opt => opt.MapFrom(s => s.Group.Subject))
            .ForMember(d => d.Teacher, opt => opt.MapFrom(s => s.Teacher))
            .ForMember(d => d.Room, opt => opt.MapFrom(s => s.Room))
            .ForMember(d => d.Students, opt => opt.MapFrom(s => s.Group.Students))
            .ForMember(d => d.LessonStatus, opt => opt.MapFrom(s => s.LessonStatus.ToString()));

        #region MinimalModels
        
        CreateMap<Subject, SubjectMinimalViewModel>()
            .ForMember(d => d.SubjectId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.SubjectName, opt => opt.MapFrom(s => s.Name));

        CreateMap<Groups, GroupMinimalViewModel>();

        CreateMap<User, UserMinimalModel>()
            .ForMember(d => d.Name, opt => opt.MapFrom(u => u.FullName));

        CreateMap<User, StudentMinimalViewModel>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(u => u.FullName));

        CreateMap<Room, RoomMinimalViewModel>();
        
        #endregion
    }
}