using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.GroupServices.Models;
using Trackademy.Application.Persistance;

namespace Trackademy.Application.GroupServices;

public class GroupService(TrackademyDbContext dbContext, IMapper mapper) : IGroupService
{

    public async Task<List<GroupsTdo>> GetAllAsync(GroupRequest model)
    {
        var group = await dbContext.Groups.ToListAsync();

        if (model.Ids != null && model.Ids.Count != 0)
        {
            group = group.Where(g => model.Ids.Contains(g.Id)).ToList();
        }

        var groupsTdo = mapper.Map<List<GroupsTdo>>(group);

        return groupsTdo;
    }
}