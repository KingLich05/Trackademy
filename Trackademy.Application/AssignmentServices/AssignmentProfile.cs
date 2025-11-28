using AutoMapper;
using Trackademy.Application.AssignmentServices.Models;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.AssignmentServices;

public class AssignmentProfile : Profile
{
    public AssignmentProfile()
    {
        CreateMap<Assignment, AssignmentDto>()
            .ForMember(d => d.Group, opt => opt.MapFrom(s => s.Group));

        CreateMap<Assignment, AssignmentDetailedDto>()
            .ForMember(d => d.Group, opt => opt.MapFrom(s => s.Group))
            .ForMember(d => d.SubjectName, opt => opt.MapFrom(s => s.Group != null && s.Group.Subject != null ? s.Group.Subject.Name : null))
            .ForMember(d => d.StudentSubmissions, opt => opt.Ignore()); // Will be populated manually in service

        CreateMap<AssignmentAddModel, Assignment>();
        
        CreateMap<AssignmentUpdateModel, Assignment>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.GroupId, opt => opt.Ignore())
            .ForMember(d => d.Group, opt => opt.Ignore())
            .ForMember(d => d.Submissions, opt => opt.Ignore());
    }
}