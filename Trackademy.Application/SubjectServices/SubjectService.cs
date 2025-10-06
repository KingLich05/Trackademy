using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Exception;
using Trackademy.Application.Shared.Models;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.SubjectServices;

public class SubjectService :
    BaseService<Subject, SubjectDto, SubjectAddModel,SubjectUpdateModel>,
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

    public override async Task<Guid> CreateAsync(SubjectAddModel dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ConflictException("Название предмета не может быть пустым.");
        }

        dto.Name = dto.Name.Trim();

        var isExists = await _context.Subjects.AnyAsync(s =>
            s.OrganizationId == dto.OrganizationId &&
            s.Name.ToLower() == dto.Name.ToLower());

        if (isExists)
        {
            throw new ConflictException($"Предмет с названием '{dto.Name}' уже существует в этой организации.");
        }

        return await base.CreateAsync(dto);
    }

    public override async Task<Guid> UpdateAsync(Guid id, SubjectUpdateModel dto)
    {
        var entity = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == id);

        if (entity is null) return Guid.Empty;

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ConflictException("Название предмета не может быть пустым.");
        }

        dto.Name = dto.Name.Trim();

        if (!dto.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase))
        {
            var isExists = await _context.Subjects.AnyAsync(s =>
                s.OrganizationId == entity.OrganizationId &&
                s.Name.ToLower() == dto.Name.ToLower() &&
                s.Id != entity.Id);

            if (isExists)
            {
                throw new ConflictException($"Предмет с названием '{dto.Name}' уже существует в этой организации.");
            }
        }

        return await base.UpdateAsync(id, dto);
    }
}