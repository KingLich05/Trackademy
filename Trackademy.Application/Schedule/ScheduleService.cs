using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.Schedule.Model;
using Trackademy.Domain.Enums;

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

        var teacher = await dbContext.Users
            .Where(x => x.Role == RoleEnum.Teacher)
            .FirstOrDefaultAsync(x => x.Id == addModel.TeacherId);
        if (teacher == null)
        {
            return false;
        }

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
        await dbContext.SaveChangesAsync();

        var countLessons = await CreateLessons(newSchedule);
        Console.WriteLine("Создано уроков: " + countLessons);

        return true;
    }

    private async Task<int> CreateLessons(Domain.Users.Schedule schedule)
    {
        var lessonsToAdd = new List<Domain.Users.Lesson>();

        var startDate = schedule.EffectiveFrom;
        var endDate = schedule.EffectiveTo ?? startDate.AddMonths(2);

        var subjectId = await dbContext.Groups
            .Where(g => g.Id == schedule.GroupId)
            .Select(g => g.SubjectId)
            .SingleAsync();

        var allowedDays = (schedule.DaysOfWeek ?? Array.Empty<int>()).ToHashSet();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dow = (int)date.DayOfWeek;
            if (dow == 0)
            { 
                // в самом .Net воскресенье это 0 из за этого мы переделываем под нашу систему
                dow = 7;
            }

            if (!allowedDays.Contains(dow))
                continue;

            lessonsToAdd.Add(new Domain.Users.Lesson
            {
                ScheduleId = schedule.Id,
                Date = date,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                SubjectId = subjectId,
                GroupId = schedule.GroupId,
                TeacherId = schedule.TeacherId,
                RoomId = schedule.RoomId,
                LessonStatus = LessonStatus.Planned
            });
        }

        if (lessonsToAdd.Count <= 0) return lessonsToAdd.Count;
        await dbContext.Lessons.AddRangeAsync(lessonsToAdd);
        await dbContext.SaveChangesAsync();

        return lessonsToAdd.Count;
    }
}