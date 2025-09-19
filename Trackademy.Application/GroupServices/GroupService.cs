using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.GroupServices.Models;
using Trackademy.Application.Persistance;
using Trackademy.Domain.Users;

namespace Trackademy.Application.GroupServices;

public class GroupService(TrackademyDbContext dbContext, IMapper mapper) : IGroupService
{

    public async Task<List<GroupsTdo>> GetAllAsync(GroupRequest model)
    {
        var group = await dbContext.Groups
            .Where(x => x.OrganizationId == model.OrganizationId)
            .ToListAsync();

        var groupsTdo = mapper.Map<List<GroupsTdo>>(group);

        return groupsTdo;
    }

    public async Task CreateGroup(GroupsAddModel model)
    {
        var group = mapper.Map<Groups>(model);

        await dbContext.Groups.AddAsync(group);
        await dbContext.SaveChangesAsync();
    }
}