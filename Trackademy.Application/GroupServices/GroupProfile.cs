using AutoMapper;
using Trackademy.Application.GroupServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.GroupServices;

public class GroupProfile : Profile
{
    public GroupProfile()
    {
        CreateMap<Groups, GroupsDto>();

        CreateMap<GroupsAddModel, Groups>()
            .ForMember(dest => dest.Subject, opt => opt.Ignore())
            .ForMember(dest => dest.Schedules, opt => opt.Ignore())
            .ForMember(dest => dest.Students, opt => opt.Ignore())
            .ForMember(dest => dest.Organization, opt => opt.Ignore());
    }
}