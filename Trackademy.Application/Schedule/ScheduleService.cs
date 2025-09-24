using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.Schedule.Model;

namespace Trackademy.Application.Schedule;

public class ScheduleService(TrackademyDbContext dbContext, IMapper mapper) : IScheduleService
{
    public async Task<bool> CreateSchedule(ScheduleAddModel addModel)
    {
        var existSchedules = await dbContext.Schedules
            .AsNoTracking()
            .Where(x => x.OrganizationId == addModel.OrganizationId)
            .ToListAsync();

        foreach (var s in existSchedules)
        {
            // 1. Проверка пересечения по дням недели
            if (s.DaysOfWeek != null && addModel.DaysOfWeek != null)
            {
                if (!s.DaysOfWeek.Intersect(addModel.DaysOfWeek).Any())
                    continue;
            }

            // 2. Проверка пересечения по времени
            var overlapByTime = s.StartTime < addModel.EndTime && addModel.StartTime < s.EndTime;
            if (!overlapByTime)
                continue; // нет пересечения по времени -> нет конфликта

            // 3. Проверка по аудитории
            if (s.RoomId == addModel.RoomId)
                return false;

            // 4. Проверка по преподавателю
            if (s.TeacherId == addModel.TeacherId)
                return false;
        }

        // Если сюда дошли — значит конфликтов нет
        var newSchedule = new Domain.Users.Schedule
        {
            DaysOfWeek = addModel.DaysOfWeek,
            StartTime = addModel.StartTime,
            EndTime = addModel.EndTime,
            EffectiveFrom = addModel.EffectiveFrom,
            EffectiveTo = addModel.EffectiveTo,
            GroupId = addModel.GroupId,
            TeacherId = addModel.TeacherId,
            RoomId = addModel.RoomId,
            OrganizationId = addModel.OrganizationId
        };

        dbContext.Schedules.Add(newSchedule);
       
        
        await dbContext.SaveChangesAsync();
        return true;
    }
}