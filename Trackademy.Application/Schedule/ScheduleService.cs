using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.Schedule.Model;

namespace Trackademy.Application.Schedule;

public class ScheduleService(TrackademyDbContext dbContext, IMapper mapper) : IScheduleService
{
    public async Task<bool> CreateSchedule(ScheduleAddModel addModel)
    {
        var start = TimeSpan.Parse(addModel.StartTime);
        var end = TimeSpan.Parse(addModel.EndTime);
        var existSchedules = await dbContext.Schedules
            .AsNoTracking()
            .Where(x => x.OrganizationId == addModel.OrganizationId)
            .ToListAsync();

        foreach (var s in existSchedules)
        {
            if (s.DaysOfWeek != null && addModel.DaysOfWeek != null)
            {
                if (!s.DaysOfWeek.Intersect(addModel.DaysOfWeek).Any())
                    continue;
            }

            var overlapByTime = s.StartTime < end && start < s.EndTime;
            if (!overlapByTime)
                continue;

            if (s.RoomId == addModel.RoomId)
                return false;

            if (s.TeacherId == addModel.TeacherId)
                return false;
        }

        var newSchedule = new Domain.Users.Schedule
        {
            DaysOfWeek = addModel.DaysOfWeek,
            StartTime = start,
            EndTime = end,
            EffectiveFrom = addModel.EffectiveFrom,
            EffectiveTo = addModel.EffectiveTo,
            GroupId = addModel.GroupId,
            TeacherId = addModel.TeacherId,
            RoomId = addModel.RoomId,
            OrganizationId = addModel.OrganizationId
        };

        await dbContext.Schedules.AddAsync(newSchedule);
        
        // теперь здесь нужно создать все записи lesson для последующих уроков.
        await CreateLessons(addModel);
       
        
        await dbContext.SaveChangesAsync();
        return true;
    }

    private async Task CreateLessons(ScheduleAddModel addModel)
    {
        
    }
}