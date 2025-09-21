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
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync();

        var groupsTdo = _mapper.Map<List<GroupsDto>>(group);

        return groupsTdo;
    }

    public async Task CreateGroup(GroupsAddModel model)
    {
        var group = _mapper.Map<Groups>(model);

        await _context.Groups.AddAsync(group);
        await _context.SaveChangesAsync();
    }
}