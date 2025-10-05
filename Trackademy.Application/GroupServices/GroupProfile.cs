using AutoMapper;
using Trackademy.Application.GroupServices.Models;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.GroupServices;

public class GroupProfile : Profile
{
    public GroupProfile()
    {
        CreateMap<User, UserMinimalViewModel>()
            .ForMember(x => x.StudentId, o => o.MapFrom(q => q.Id))
            .ForMember(x => x.StudentName, o => o.MapFrom(q => q.FullName));

        CreateMap<Subject, SubjectMinimalViewModel>()
            .ForMember(x => x.SubjectId, o => o.MapFrom(q => q.Id))
            .ForMember(x => x.SubjectName, o => o.MapFrom(q => q.Name));

        CreateMap<Groups, GroupsDto>();

        CreateMap<GroupsUpdateModel, Groups>()
            .ForMember(dest => dest.Subject, opt => opt.Ignore())
            .ForMember(dest => dest.Schedules, opt => opt.Ignore())
            .ForMember(dest => dest.Students, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore());

        CreateMap<GroupsAddModel, Groups>()
            .ForMember(dest => dest.Subject, opt => opt.Ignore())
            .ForMember(dest => dest.Schedules, opt => opt.Ignore())
            .ForMember(dest => dest.Students, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore());
    }
}