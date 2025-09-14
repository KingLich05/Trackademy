using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Models;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.SubjectServices;

public class SubjectService :
    BaseService<Subject, SubjectDto, SubjectAddModel>,
    ISubjectService
{
    private TrackademyDbContext _context;
    private readonly IMapper _mapper;

    public SubjectService(TrackademyDbContext context, IMapper mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SubjectDto>> GetAllAsync(RequestIdOrganization request)
    {
        var subjects = await _context.Subjects
            .Where(x => x.OrganizationId == request.OrganizationId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<SubjectDto>>(subjects);
    }
}