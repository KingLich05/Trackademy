using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trackademy.Application.Persistance;
using Trackademy.Application.Schedule.Model;
using Trackademy.Application.Shared.Exception;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Schedule;

public class ScheduleService(
    TrackademyDbContext dbContext,
    IMapper mapper,
    ILogger<ScheduleService> logger) : IScheduleService
{
    public async Task<ScheduleViewModel?> GetSchedule(Guid id)
    {
        var schedule = await dbContext.Schedules
            .Where(x => x.Id == id)
            .Include(x => x.Teacher)
            .Include(x => x.Room)
            .Include(x => x.Group)
                .ThenInclude(x => x.Subject)
            .FirstOrDefaultAsync();

        if (schedule == null)
            return null;

        // Загружаем уроки отдельно, так как у Schedule нет прямой связи с Lesson
        var lessons = await dbContext.Lessons
            .Where(l => l.ScheduleId == schedule.Id)
            .Include(l => l.Group)
                .ThenInclude(g => g.Subject)
            .Include(l => l.Teacher)
            .Include(l => l.Room)
            .ToListAsync();

        var result = mapper.Map<ScheduleViewModel>(schedule);
        result.Lessons = mapper.Map<List<LessonViewModel>>(lessons);

        return result;
    }

    public async Task<List<ScheduleViewModel>> GetAllSchedulesAsync(ScheduleRequest scheduleRequest)
    {
        var scheduleQuery = dbContext.Schedules
            .Where(x => x.OrganizationId == scheduleRequest.OrganizationId)
            .Include(x => x.Teacher)
            .Include(x => x.Room)
            .Include(x => x.Group)
            .ThenInclude(x => x.Subject)
            .AsQueryable();

        var schedule = await Filtration(scheduleRequest, scheduleQuery);

        return mapper.Map<List<ScheduleViewModel>>(schedule);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await dbContext.Schedules.FindAsync(id);
        if (entity == null)
            return false;

        var today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Asia/Almaty")));

        var hasPastLessons = await dbContext.Lessons
            .AnyAsync(l => l.ScheduleId == id && l.Date < today);

        if (hasPastLessons)
        {
            entity.EffectiveTo = today.AddDays(-1);

            var count = await dbContext.Lessons
                .Where(l => l.ScheduleId == id && l.Date > entity.EffectiveTo)
                .ExecuteDeleteAsync();
            await dbContext.SaveChangesAsync();

            logger.LogInformation("было удалено будущих уроков: {name}", count);

            return true;
        }

        dbContext.Schedules.Remove(entity);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Guid> UpdateScheduleAsync(
        Guid id,
        ScheduleUpdateModel updateModel)
    {
        var start = updateModel.StartTime;
        var end = updateModel.EndTime;

        var existingSchedule = await dbContext.Schedules
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existingSchedule == null)
        {
            throw new ConflictException.NotFoundException("Расписание не найдено.");
        }

        var room = await dbContext.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == updateModel.RoomId);

        var group = await dbContext.Groups
            .AsNoTracking()
            .Include(g => g.Students)
            .FirstOrDefaultAsync(x => x.Id == updateModel.GroupId);

        var teacher = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Role == RoleEnum.Teacher)
            .FirstOrDefaultAsync(x => x.Id == updateModel.TeacherId);

        if (room == null)
        {
            throw new ConflictException("Кабинета не существует.");
        }

        if (group == null)
        {
            throw new ConflictException("Группы не существует.");
        }

        if (teacher == null)
        {
            throw new ConflictException("Преподавателя не существует.");
        }

        if (room.Capacity < group.Students.Count)
        {
            throw new ConflictException("Количество студентов не вмещается в кабинет.");
        }

        var existOtherSchedules = await dbContext.Schedules
            .AsNoTracking()
            .Where(x => x.Id != id)
            .ToListAsync();

        foreach (var s in existOtherSchedules)
        {
            if (s.DaysOfWeek != null && updateModel.DaysOfWeek != null)
            {
                if (!s.DaysOfWeek.Intersect(updateModel.DaysOfWeek).Any())
                    continue;
            }

            var overlapByTime = s.StartTime < end && start < s.EndTime;
            if (!overlapByTime) continue;

            if (s.RoomId == updateModel.RoomId)
                throw new ConflictException("Кабинет в это время занят.");

            if (s.TeacherId == updateModel.TeacherId)
                throw new ConflictException("Преподаватель в это время занят.");
        }

        mapper.Map(updateModel, existingSchedule);

        await dbContext.SaveChangesAsync();

        await ReplaceFutureLessonsAsync(existingSchedule);

        return existingSchedule.Id;
    }

    public async Task<Guid> CreateSchedule(ScheduleAddModel addModel)
    {
        var start = addModel.StartTime;
        var end = addModel.EndTime;

        var room = await dbContext.Rooms
            .AsNoTracking()
            .FirstAsync(x => x.Id == addModel.RoomId);

        var group = await dbContext.Groups
            .AsNoTracking()
            .Include(groups => groups.Students)
            .FirstAsync(x => x.Id == addModel.GroupId);

        if (room.Capacity < group.Students.Count)
        {
            throw new ConflictException($"Количество студентов не вмещается в кабинет.");
        }

        var existSchedules = await dbContext.Schedules
            .AsNoTracking()
            .Where(x => x.OrganizationId == addModel.OrganizationId)
            .ToListAsync();

        var teacher = await dbContext.Users
            .Where(x => x.Role == RoleEnum.Teacher)
            .FirstOrDefaultAsync(x => x.Id == addModel.TeacherId);
        if (teacher == null)
        {
            throw new ConflictException("Преподавателя не существует.");
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
                throw new ConflictException("Кабинет в это время занят.");

            if (s.TeacherId == addModel.TeacherId)
                throw new ConflictException("Преподаватель в это время занят.");
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

        var countLessons = await CreateLessonsAsync(newSchedule);
        Console.WriteLine("Создано уроков: " + countLessons);

        return newSchedule.Id;
    }

    #region Private methods

    private async Task ReplaceFutureLessonsAsync(Domain.Users.Schedule schedule)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Almaty");
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));

        var futureLessons = await dbContext.Lessons
            .Where(l => l.ScheduleId == schedule.Id && l.Date >= today)
            .ToListAsync();

        if (futureLessons.Any())
        {
            dbContext.Lessons.RemoveRange(futureLessons);
            await dbContext.SaveChangesAsync();
        }

        await CreateLessonsAsync(schedule, DateTime.Now);
    }

    private async Task<List<Domain.Users.Schedule>> Filtration(
        ScheduleRequest req,
        IQueryable<Domain.Users.Schedule> schedules)
    {
        if (req.SubjectId.HasValue && req.SubjectId != Guid.Empty)
        {
            schedules = schedules.Where(x => x.Group.SubjectId == req.SubjectId);
        }

        if (req.RoomId.HasValue && req.RoomId != Guid.Empty)
        {
            schedules = schedules.Where(x => x.RoomId == req.RoomId);
        }

        if (req.TeacherId.HasValue && req.TeacherId != Guid.Empty)
        {
            schedules = schedules.Where(x => x.TeacherId == req.TeacherId);
        }

        if (req.GroupId.HasValue && req.GroupId != Guid.Empty)
        {
            schedules = schedules.Where(x => x.GroupId == req.GroupId);
        }

        return await schedules.ToListAsync();
    }

    private async Task<int> CreateLessonsAsync(Domain.Users.Schedule schedule, DateTime? overrideStartDate = null)
    {
        var lessonsToAdd = new List<Domain.Users.Lesson>();

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Almaty");
        var utcNow = DateTime.UtcNow;
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
        var todayDateOnly = DateOnly.FromDateTime(localNow);

        logger.LogInformation("CreateLessons now: {Now}", localNow);

        var startDate = overrideStartDate.HasValue
            ? DateOnly.FromDateTime(overrideStartDate.Value)
            : schedule.EffectiveFrom;

        if (startDate < todayDateOnly)
            startDate = todayDateOnly;

        var endDate = schedule.EffectiveTo ?? startDate.AddMonths(2);

        var currentDayNumber = localNow.DayOfWeek switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            DayOfWeek.Saturday => 6,
            DayOfWeek.Sunday => 7,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (schedule.DaysOfWeek != null &&
            schedule.DaysOfWeek.Contains(currentDayNumber) &&
            localNow.TimeOfDay > schedule.EndTime)
        {
            startDate = startDate.AddDays(1);
        }

        var allowedDays = (schedule.DaysOfWeek ?? []).ToHashSet();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dow = (int)date.DayOfWeek;
            if (dow == 0) dow = 7; // Преобразуем Sunday (0) → 7

            if (!allowedDays.Contains(dow))
                continue;

            lessonsToAdd.Add(new Domain.Users.Lesson
            {
                ScheduleId = schedule.Id,
                Date = date,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                GroupId = schedule.GroupId,
                TeacherId = schedule.TeacherId,
                RoomId = schedule.RoomId,
                LessonStatus = LessonStatus.Planned
            });
        }

        if (lessonsToAdd.Count == 0)
            return 0;

        await dbContext.Lessons.AddRangeAsync(lessonsToAdd);
        await dbContext.SaveChangesAsync();

        return lessonsToAdd.Count;
    }

    #endregion
}