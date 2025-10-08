using AutoMapper;
using Trackademy.Application.RoomServices.Models;
using Trackademy.Application.Schedule.Model;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Schedule;

public class ScheduleProfile : Profile
{
    public ScheduleProfile()
    {
        CreateMap<Domain.Users.Schedule, ScheduleViewModel>()
            .ForMember(d => d.Subject, opt => opt.MapFrom(s => s.Group.Subject))
            .ForMember(d => d.Group, opt => opt.MapFrom(s => s.Group))
            .ForMember(d => d.Teacher, opt => opt.MapFrom(s => s.Teacher))
            .ForMember(d => d.Room, opt => opt.MapFrom(s => s.Room));

        CreateMap<ScheduleUpdateModel, Domain.Users.Schedule>()
            .ForMember(x => x.Group, opt => opt.Ignore())
            .ForMember(x => x.Teacher, opt => opt.Ignore())
            .ForMember(x => x.Room, opt => opt.Ignore())
            .ForMember(x => x.Organization, opt => opt.Ignore())
            .ForMember(x => x.OrganizationId, opt => opt.Ignore());

        #region MinimalViewModel

        CreateMap<Subject, SubjectMinimalViewModel>()
            .ForMember(d => d.SubjectId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.SubjectName, opt => opt.MapFrom(s => s.Name));

        CreateMap<Groups, GroupMinimalViewModel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(g => g.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(g => g.Name));

        CreateMap<User, UserMinimalModel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(u => u.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(u => u.FullName));

        CreateMap<Room, RoomMinimalViewModel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(r => r.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(r => r.Name));

        #endregion
    }
}