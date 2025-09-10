using AutoMapper;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.SubjectServices;

public class SubjectService : 
    BaseService<Subject, SubjectDto, SubjectAddModel>,
    ISubjectService
{
    public SubjectService(TrackademyDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
    }
}