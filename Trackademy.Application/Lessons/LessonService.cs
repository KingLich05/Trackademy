using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Lessons.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Schedule.Model;
using Trackademy.Application.Shared.Exception;
using Trackademy.Application.Shared.Extensions;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Lessons;

public class LessonService(
    TrackademyDbContext dbContext,
    IMapper mapper) : ILessonService
{
    public async Task<Guid> CreateCustomLessonAsync(LessonCustomAddModel model)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Almaty");
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));

        if (model.Date < today)
        {
            throw new ConflictException("Нельзя создать урок в прошлом.");
        }

        if (model.EndTime <= model.StartTime)
        {
            throw new ConflictException("Время окончания должно быть позже начала.");
        }

        var groupExists = await dbContext.Groups.AnyAsync(x => x.Id == model.GroupId);
        var teacherExists = await dbContext.Users.AnyAsync(x => x.Id == model.TeacherId && x.Role == RoleEnum.Teacher);
        var room = await dbContext.Rooms.FirstOrDefaultAsync(x => x.Id == model.RoomId);

        if (!groupExists || !teacherExists || room == null)
        {
            throw new ConflictException("Проверьте корректность группы, преподавателя или кабинета.");
        }

        var studentCount = await dbContext.Groups
            .Where(x => x.Id == model.GroupId)
            .SelectMany(g => g.Students)
            .CountAsync();

        if (room.Capacity < studentCount)
        {
            throw new ConflictException("Количество студентов не вмещается в кабинет.");
        }

        var overlapExists = await dbContext.Lessons
            .Where(l => l.Date == model.Date)
            .Where(l =>
                l.TeacherId == model.TeacherId ||
                l.GroupId == model.GroupId ||
                l.RoomId == model.RoomId)
            .AnyAsync(l =>
                l.StartTime < new TimeSpan(model.EndTime.Ticks) && new TimeSpan(model.StartTime.Ticks) < l.EndTime);

        if (overlapExists)
        {
            throw new ConflictException("Указанное время пересекается с другим занятием.");
        }

        var newLesson = new Lesson
        {
            Id = Guid.NewGuid(),
            Date = model.Date,
            StartTime = new TimeSpan(model.StartTime.Ticks),
            EndTime = new TimeSpan(model.EndTime.Ticks),
            GroupId = model.GroupId,
            TeacherId = model.TeacherId,
            RoomId = model.RoomId,
            ScheduleId = model.ScheduleId,
            LessonStatus = LessonStatus.Planned,
            Note = model.Note
        };

        await dbContext.Lessons.AddAsync(newLesson);
        await dbContext.SaveChangesAsync();

        return newLesson.Id;
    }

    public async Task<bool> RescheduleLessonAsync(Guid lessonId, LessonRescheduleModel model)
    {
        if (model.CancelReason == null)
        {
            throw new ConflictException("Заполните поле причину переноса занятия.");
        }

        var lesson = await dbContext.Lessons
            .AsTracking()
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null)
            return false;

        if (lesson.LessonStatus == LessonStatus.Completed)
        {
            throw new ConflictException("Нельзя перенести завершенный урок.");
        }

        if (model.EndTime <= model.StartTime)
            throw new ConflictException("Время окончания должно быть позже времени начала.");

        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Almaty"));

        var lessonDateTime = new DateTime(
            model.Date.Year, model.Date.Month, model.Date.Day,
            model.EndTime.Hours, model.EndTime.Minutes, model.EndTime.Seconds);
        if (lessonDateTime < now)
        {
            throw new ConflictException("Нельзя перенести урок в прошлое.");
        }

        var overlapExists = await dbContext.Lessons
            .Where(l => l.Id != lesson.Id && l.Date == model.Date)
            .Where(l =>
                l.TeacherId == lesson.TeacherId ||
                l.GroupId == lesson.GroupId ||
                l.RoomId == lesson.RoomId)
            .AnyAsync(l =>
                l.StartTime < model.EndTime && model.StartTime < l.EndTime);

        if (overlapExists)
            throw new ConflictException("На выбранное время уже назначен другой урок.");

        lesson.Date = model.Date;
        lesson.StartTime = model.StartTime;
        lesson.EndTime = model.EndTime;
        lesson.CancelReason = model.CancelReason;
        lesson.LessonStatus = LessonStatus.Moved;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLessonStatusAsync(Guid lessonId, LessonStatus newStatus, string? cancelReason = null)
    {
        var lesson = await dbContext.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null)
            return false;

        // Проверяем, что нельзя завершать уроки в будущем
        if (newStatus == LessonStatus.Completed)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Almaty");
            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
            
            if (lesson.Date > today)
            {
                throw new ConflictException("Нельзя завершить урок, который еще не прошел.");
            }
        }

        lesson.LessonStatus = newStatus;
        
        // Если урок отменяется, сохраняем причину отмены
        if (newStatus == LessonStatus.Cancelled && cancelReason != null)
        {
            lesson.CancelReason = cancelReason;
        }
    
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<LessonViewModel>> GetLessonsByScheduleAsync(
        Guid scheduleId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null)
    {
        var query = dbContext.Lessons
            .Include(x => x.Group)
                .ThenInclude(g => g.Subject)
            .Include(x => x.Group)
                .ThenInclude(g => g.Students)
            .Include(x => x.Teacher)
            .Include(x => x.Room)
            .Include(x => x.Attendances)
            .Where(x => x.ScheduleId == scheduleId);

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.Date <= toDate.Value);
        }

        var lessons = await query.OrderBy(x => x.Date).ToListAsync();

        return mapper.Map<List<LessonViewModel>>(lessons);
    }

    public async Task<LessonViewModel> GetLessonByIdAsync(Guid id)
    {
        var lesson = await dbContext.Lessons
            .Include(x => x.Group)
                .ThenInclude(g => g.Subject)
            .Include(x => x.Group)
                .ThenInclude(g => g.Students)
            .Include(x => x.Teacher)
            .Include(x => x.Room)
            .Include(x => x.Attendances)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (lesson == null)
        {
            throw new ConflictException.NotFoundException("Урок не найден.");
        }

        return mapper.Map<LessonViewModel>(lesson);
    }

    public async Task<PagedResult<LessonViewModel>> GetLessonsByScheduleAsync(GetLessonsByScheduleRequest request)
    {
        var query = dbContext.Lessons
            .Include(x => x.Group)
                .ThenInclude(g => g.Subject)
            .Include(x => x.Group)
                .ThenInclude(g => g.Students)
            .Include(x => x.Teacher)
            .Include(x => x.Room)
            .Include(x => x.Attendances)
            .Where(x => x.Group.OrganizationId == request.OrganizationId);

        if (request.ScheduleId.HasValue)
        {
            query = query.Where(x => x.ScheduleId == request.ScheduleId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.Date >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.Date <= request.ToDate.Value);
        }

        var pagedLessons = await query
            .OrderBy(x => x.Date)
            .Select(lesson => mapper.Map<LessonViewModel>(lesson))
            .ToPagedResultAsync(request);

        return pagedLessons;
    }

    public async Task<Guid> UpdateLessonNoteAsync(Guid lessonId, string note)
    {
        var lesson = await dbContext.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null)
        {
            throw new ConflictException("Урок не найден.");
        }

        lesson.Note = note;
        await dbContext.SaveChangesAsync();
        return lessonId;
    }
}