using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.GroupServices.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Domain.Users;

namespace Trackademy.Application.GroupServices;

public class GroupService:
    BaseService<Groups, GroupsDto, GroupsAddModel>, IGroupService
{
    private TrackademyDbContext _context;
    private readonly IMapper _mapper;
    
    public GroupService(TrackademyDbContext context, IMapper mapper)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public override async Task<bool> UpdateAsync(Guid id, GroupsAddModel dto)
    {
        var entity = await _context.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (entity is null) return false;

        _mapper.Map(dto, entity);

        if (dto.StudentIds is not null)
        {
            var desired = dto.StudentIds.Distinct().ToHashSet();
            var current = entity.Students.Select(s => s.Id).ToHashSet();

            var idsToAdd    = desired.Except(current).ToList();
            var idsToRemove = current.Except(desired).ToList();

            if (idsToRemove.Count > 0)
            {
                var removeSet = idsToRemove.ToHashSet();
                entity.Students = entity.Students
                    .Where(s => !removeSet.Contains(s.Id))
                    .ToList();
            }

            if (idsToAdd.Count > 0)
            {
                var usersToAdd = await _context.Users
                    .Where(u => idsToAdd.Contains(u.Id))
                    .ToListAsync();

                foreach (var u in usersToAdd)
                {
                    entity.Students.Add(u);
                }
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<GroupsDto>> GetAllAsync(Guid organizationId)
    {
        var group = await _context.Groups
            .Include(x => x.Students)
            .Include(x => x.Subject)
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var groupsTdo = _mapper.Map<List<GroupsDto>>(group);

        return groupsTdo;
    }

    public async Task CreateGroup(GroupsAddModel model)
    {
        if (string.IsNullOrEmpty(model.Code) || string.IsNullOrWhiteSpace(model.Code))
        {
            model.Code = GenerateCode();
        }

        if (string.IsNullOrEmpty(model.Name) || string.IsNullOrWhiteSpace(model.Name))
        {
            var subject = await _context.Subjects.FindAsync(model.SubjectId);
            model.Name = subject?.Name + "-" + model.Code;
        }

        var group = _mapper.Map<Groups>(model);
        if (model.StudentIds.Count != 0)
        {
            var students = await _context.Users
                .Where(u => model.StudentIds.Contains(u.Id))
                .ToListAsync();

            group.Students = students;
        }

        await _context.Groups.AddAsync(group);
        await _context.SaveChangesAsync();
    }

    private static string GenerateCode()
    {
        var random = new Random();

        var letters = new string(Enumerable.Range(0, 2)
            .Select(_ => (char)random.Next('A', 'Z' + 1))
            .ToArray());

        var numbers = random.Next(0, 1000).ToString("D3");

        return $"{letters}-{numbers}";
    }
}