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
}