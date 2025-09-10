using AutoMapper;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.SubjectServices;

public class SubjectProfile : Profile
{
    public SubjectProfile()
    {
        CreateMap<Subject, SubjectDto>();
        CreateMap<SubjectAddModel, Subject>()
            .ForMember(x => x.Assignments, opt => opt.Ignore())
            .ForMember(x => x.Groups, opt => opt.Ignore());
    }
}