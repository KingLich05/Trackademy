using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Dashboard.Models;
using Trackademy.Application.Persistance;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Dashboard;

/// <summary>
/// Сервис для работы с дашбордом и аналитикой
/// </summary>
public class DashboardService(TrackademyDbContext dbContext) : IDashboardService
{
    /// <summary>
    /// Вспомогательный метод для получения OrganizationId из фильтра
    /// </summary>
    private Guid GetOrganizationIdFromFilter(DashboardFilterDto? filter)
    {
        if (filter?.OrganizationId == null)
        {
            throw new ArgumentException("OrganizationId is required in filter");
        }
        return filter.OrganizationId.Value;
    }

    /// <summary>
    /// Получить полную статистику дашборда
    /// </summary>
    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(DashboardFilterDto? filter = null)
    {
        // Получаем все данные параллельно для лучшей производительности
        var studentStatsTask = GetStudentStatsAsync(filter);
        var groupStatsTask = GetGroupStatsAsync(filter);
        var lessonStatsTask = GetLessonStatsAsync(filter);
        var attendanceStatsTask = GetAttendanceStatsAsync(filter);
        var lowPerformanceGroupsTask = GetLowPerformanceGroupsAsync(filter);
        var unpaidStudentsTask = GetUnpaidStudentsAsync(filter);
        var trialStudentsTask = GetTrialStudentsAsync(filter);
        var topTeachersTask = GetTopTeachersAsync(filter, 5);
        var latestScheduleUpdateTask = GetLatestScheduleUpdateAsync(filter);

        await Task.WhenAll(
            studentStatsTask, groupStatsTask, lessonStatsTask, attendanceStatsTask,
            lowPerformanceGroupsTask, unpaidStudentsTask, trialStudentsTask,
            topTeachersTask, latestScheduleUpdateTask
        );

        return new DashboardOverviewDto
        {
            StudentStats = studentStatsTask.Result,
            GroupStats = groupStatsTask.Result,
            LessonStats = lessonStatsTask.Result,
            AttendanceStats = attendanceStatsTask.Result,
            LowPerformanceGroups = lowPerformanceGroupsTask.Result,
            UnpaidStudents = unpaidStudentsTask.Result,
            TrialStudents = trialStudentsTask.Result,
            TopTeachers = topTeachersTask.Result,
            LatestScheduleUpdate = latestScheduleUpdateTask.Result,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Получить статистику по студентам
    /// </summary>
    public async Task<StudentStatsDto> GetStudentStatsAsync(DashboardFilterDto? filter = null)
    {
        var query = dbContext.Users.AsQueryable();

        // Применяем фильтры
        if (filter?.OrganizationId.HasValue == true)
        {
            query = query.Where(u => u.OrganizationId == filter.OrganizationId.Value);
        }

        // Общее количество студентов
        var totalStudents = await query
            .Where(u => u.Role == RoleEnum.Student)
            .CountAsync();

        // Активные студенты (имеют записи посещаемости за последние 30 дней)
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var activeStudents = await query
            .Where(u => u.Role == RoleEnum.Student)
            .Where(u => dbContext.Attendances
                .Any(a => a.StudentId == u.Id && a.Date >= thirtyDaysAgo))
            .CountAsync();

        // Новые студенты за текущий месяц
        var startOfMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var newStudentsThisMonth = await query
            .Where(u => u.Role == RoleEnum.Student)
            .Where(u => DateOnly.FromDateTime(u.CreatedDate) >= startOfMonth)
            .CountAsync();

        return new StudentStatsDto
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            NewStudentsThisMonth = newStudentsThisMonth
        };
    }

    /// <summary>
    /// Получить статистику по группам
    /// </summary>
    public async Task<GroupStatsDto> GetGroupStatsAsync(DashboardFilterDto? filter = null)
    {
        var query = dbContext.Groups.AsQueryable();

        // Применяем фильтры
        if (filter?.OrganizationId.HasValue == true)
        {
            query = query.Where(g => g.OrganizationId == filter.OrganizationId.Value);
        }

        if (filter?.GroupIds?.Any() == true)
        {
            query = query.Where(g => filter.GroupIds.Contains(g.Id));
        }

        // Общее количество групп
        var totalGroups = await query.CountAsync();

        // Активные группы (у которых есть уроки в ближайшие 7 дней)
        var nextWeek = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var activeGroups = await query
            .Where(g => dbContext.Lessons
                .Any(l => l.GroupId == g.Id && l.Date >= today && l.Date <= nextWeek))
            .CountAsync();

        // Средний размер группы
        var averageGroupSize = totalGroups > 0 
            ? await query
                .Include(g => g.Students)
                .AverageAsync(g => g.Students.Count)
            : 0;

        return new GroupStatsDto
        {
            TotalGroups = totalGroups,
            ActiveGroups = activeGroups,
            AverageGroupSize = (decimal)averageGroupSize
        };
    }

    /// <summary>
    /// Получить статистику по урокам
    /// </summary>
    public async Task<LessonStatsDto> GetLessonStatsAsync(DashboardFilterDto? filter = null)
    {
        var query = dbContext.Lessons.AsQueryable();

        // Применяем фильтры
        if (filter?.OrganizationId.HasValue == true)
        {
            query = query.Where(l => dbContext.Groups
                .Any(g => g.Id == l.GroupId && g.OrganizationId == filter.OrganizationId.Value));
        }

        if (filter?.GroupIds?.Any() == true)
        {
            query = query.Where(l => filter.GroupIds.Contains(l.GroupId));
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        // Уроки на сегодня
        var lessonsToday = await query
            .Where(l => l.Date == today)
            .CountAsync();

        // Завершенные уроки сегодня
        var completedLessonsToday = await query
            .Where(l => l.Date == today && l.LessonStatus == LessonStatus.Completed)
            .CountAsync();

        // Отмененные уроки сегодня
        var cancelledLessonsToday = await query
            .Where(l => l.Date == today && l.LessonStatus == LessonStatus.Cancelled)
            .CountAsync();

        // Уроки за текущий месяц
        var lessonsThisMonth = await query
            .Where(l => l.Date >= startOfMonth)
            .CountAsync();

        return new LessonStatsDto
        {
            LessonsToday = lessonsToday,
            CompletedLessonsToday = completedLessonsToday,
            CancelledLessonsToday = cancelledLessonsToday,
            LessonsThisMonth = lessonsThisMonth
        };
    }

    /// <summary>
    /// Получить статистику по посещаемости
    /// </summary>
    public async Task<AttendanceStatsDto> GetAttendanceStatsAsync(DashboardFilterDto? filter = null)
    {
        var query = dbContext.Attendances
            .Include(a => a.Lesson)
            .AsQueryable();

        // Применяем фильтры
        if (filter?.OrganizationId.HasValue == true)
        {
            query = query.Where(a => dbContext.Groups
                .Any(g => g.Id == a.Lesson.GroupId && g.OrganizationId == filter.OrganizationId.Value));
        }

        if (filter?.GroupIds?.Any() == true)
        {
            query = query.Where(a => filter.GroupIds.Contains(a.Lesson.GroupId));
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

        // Общая посещаемость за последние 30 дней
        var totalAttendances = await query
            .Where(a => a.Date >= thirtyDaysAgo)
            .CountAsync();

        var presentAttendances = await query
            .Where(a => a.Date >= thirtyDaysAgo && a.Status == AttendanceStatus.Attend)
            .CountAsync();

        var overallAttendanceRate = totalAttendances > 0 
            ? (decimal)presentAttendances / totalAttendances * 100 
            : 0;

        // Статистика на сегодня
        var presentStudentsToday = await query
            .Where(a => a.Date == today && a.Status == AttendanceStatus.Attend)
            .CountAsync();

        var absentStudentsToday = await query
            .Where(a => a.Date == today && a.Status == AttendanceStatus.NotAttend)
            .CountAsync();

        var lateStudentsToday = await query
            .Where(a => a.Date == today && a.Status == AttendanceStatus.Late)
            .CountAsync();

        // Посещаемость по группам
        var groupAttendanceRates = await dbContext.Groups
            .Where(g => filter == null || !filter.OrganizationId.HasValue || g.OrganizationId == filter.OrganizationId.Value)
            .Include(g => g.Students)
            .Select(g => new GroupAttendanceDto
            {
                GroupId = g.Id,
                GroupName = g.Name,
                GroupCode = g.Code,
                TotalStudents = g.Students.Count,
                ActiveStudents = dbContext.Attendances
                    .Where(a => a.Lesson.GroupId == g.Id && a.Date >= thirtyDaysAgo)
                    .Select(a => a.StudentId)
                    .Distinct()
                    .Count(),
                AttendanceRate = dbContext.Attendances
                    .Where(a => a.Lesson.GroupId == g.Id && a.Date >= thirtyDaysAgo)
                    .Any() 
                    ? (decimal)dbContext.Attendances
                        .Where(a => a.Lesson.GroupId == g.Id && a.Date >= thirtyDaysAgo && a.Status == AttendanceStatus.Attend)
                        .Count() * 100 /
                      dbContext.Attendances
                        .Where(a => a.Lesson.GroupId == g.Id && a.Date >= thirtyDaysAgo)
                        .Count()
                    : 0
            })
            .ToListAsync();

        return new AttendanceStatsDto
        {
            OverallAttendanceRate = overallAttendanceRate,
            PresentStudentsToday = presentStudentsToday,
            AbsentStudentsToday = absentStudentsToday,
            LateStudentsToday = lateStudentsToday,
            GroupAttendanceRates = groupAttendanceRates
        };
    }

    public async Task<List<LowPerformanceGroupDto>> GetLowPerformanceGroupsAsync(DashboardFilterDto? filter = null)
    {
        var organizationId = GetOrganizationIdFromFilter(filter);
        
        var query = dbContext.Groups
            .Include(g => g.Subject)
            .Include(g => g.Students)
            .Include(g => g.Lessons)
                .ThenInclude(l => l.Attendances)
            .Where(g => g.OrganizationId == organizationId);

        if (filter?.GroupIds != null && filter.GroupIds.Any())
        {
            query = query.Where(g => filter.GroupIds.Contains(g.Id));
        }

        var groups = await query.ToListAsync();

        var lowPerformanceGroups = new List<LowPerformanceGroupDto>();

        foreach (var group in groups)
        {
            var totalLessons = group.Lessons.Count;
            if (totalLessons == 0) continue;

            var totalAttendances = group.Lessons.Sum(l => l.Attendances.Count(a => a.Status == AttendanceStatus.Present));
            var totalPossibleAttendances = totalLessons * group.Students.Count;

            if (totalPossibleAttendances == 0) continue;

            var attendanceRate = (double)totalAttendances / totalPossibleAttendances * 100;

            if (attendanceRate < 70) // Группы с посещаемостью менее 70%
            {
                lowPerformanceGroups.Add(new LowPerformanceGroupDto
                {
                    GroupId = group.Id,
                    GroupName = group.Name,
                    SubjectName = group.Subject.Name,
                    AttendanceRate = attendanceRate,
                    StudentCount = group.Students.Count,
                    TotalLessons = totalLessons
                });
            }
        }

        return lowPerformanceGroups.OrderBy(x => x.AttendanceRate).ToList();
    }

    public async Task<List<UnpaidStudentDto>> GetUnpaidStudentsAsync(DashboardFilterDto? filter = null)
    {
        var organizationId = GetOrganizationIdFromFilter(filter);
        
        var query = dbContext.Users
            .Include(u => u.Groups)
                .ThenInclude(g => g.Subject)
            .Where(u => u.OrganizationId == organizationId && 
                       u.Role == RoleEnum.Student && 
                       u.PaymentStatus == PaymentStatus.Unpaid);

        if (filter?.StartDate.HasValue == true)
        {
            query = query.Where(u => u.CreatedDate >= filter.StartDate.Value);
        }

        if (filter?.EndDate.HasValue == true)
        {
            query = query.Where(u => u.CreatedDate <= filter.EndDate.Value);
        }

        var unpaidStudents = await query
            .Select(u => new UnpaidStudentDto
            {
                StudentId = u.Id,
                StudentName = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Groups = u.Groups.Select(g => g.Name).ToList(),
                DebtAmount = 0, // Здесь нужно будет добавить логику расчета долга
                DaysOverdue = (DateTime.Now - u.CreatedDate).Days
            })
            .OrderByDescending(x => x.DaysOverdue)
            .ToListAsync();

        return unpaidStudents;
    }

    public async Task<List<TrialStudentDto>> GetTrialStudentsAsync(DashboardFilterDto? filter = null)
    {
        var organizationId = GetOrganizationIdFromFilter(filter);
        
        var query = dbContext.Users
            .Include(u => u.Groups)
                .ThenInclude(g => g.Subject)
            .Where(u => u.OrganizationId == organizationId && 
                       u.Role == RoleEnum.Student && 
                       u.StatusStudent == StatusStudent.Trial);

        if (filter?.StartDate.HasValue == true)
        {
            query = query.Where(u => u.CreatedDate >= filter.StartDate.Value);
        }

        if (filter?.EndDate.HasValue == true)
        {
            query = query.Where(u => u.CreatedDate <= filter.EndDate.Value);
        }

        var trialStudents = await query
            .Select(u => new TrialStudentDto
            {
                StudentId = u.Id,
                StudentName = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Groups = u.Groups.Select(g => g.Name).ToList(),
                TrialStartDate = u.CreatedDate,
                DaysInTrial = (DateTime.Now - u.CreatedDate).Days
            })
            .OrderByDescending(x => x.DaysInTrial)
            .ToListAsync();

        return trialStudents;
    }

    public async Task<List<TopTeacherDto>> GetTopTeachersAsync(DashboardFilterDto? filter = null, int limit = 10)
    {
        var organizationId = GetOrganizationIdFromFilter(filter);
        
        var query = dbContext.Users
            .Include(u => u.TeacherGroups)
                .ThenInclude(g => g.Lessons)
                    .ThenInclude(l => l.Attendances)
            .Include(u => u.TeacherGroups)
                .ThenInclude(g => g.Subject)
            .Include(u => u.TeacherGroups)
                .ThenInclude(g => g.Students)
            .Where(u => u.OrganizationId == organizationId && u.Role == RoleEnum.Teacher);

        var teachers = await query.ToListAsync();

        var topTeachers = new List<TopTeacherDto>();

        foreach (var teacher in teachers)
        {
            var groups = teacher.TeacherGroups;
            var totalLessons = groups.Sum(g => g.Lessons.Count);
            var totalStudents = groups.Sum(g => g.Students.Count);

            if (totalLessons == 0) continue;

            var totalAttendances = groups
                .SelectMany(g => g.Lessons)
                .Sum(l => l.Attendances.Count(a => a.Status == AttendanceStatus.Present));

            var totalPossibleAttendances = groups
                .SelectMany(g => g.Lessons)
                .Sum(l => g.Students.Count);

            var attendanceRate = totalPossibleAttendances > 0 
                ? (double)totalAttendances / totalPossibleAttendances * 100 
                : 0;

            topTeachers.Add(new TopTeacherDto
            {
                TeacherId = teacher.Id,
                TeacherName = teacher.Name,
                Email = teacher.Email,
                GroupCount = groups.Count,
                StudentCount = totalStudents,
                TotalLessons = totalLessons,
                AttendanceRate = attendanceRate,
                Subjects = groups.Select(g => g.Subject.Name).Distinct().ToList()
            });
        }

        return topTeachers
            .OrderByDescending(x => x.AttendanceRate)
            .ThenByDescending(x => x.StudentCount)
            .Take(limit)
            .ToList();
    }

    public async Task<LatestScheduleUpdateDto?> GetLatestScheduleUpdateAsync(DashboardFilterDto? filter = null)
    {
        var organizationId = GetOrganizationIdFromFilter(filter);
        
        var latestUpdate = await dbContext.Schedules
            .Include(s => s.Subject)
            .Include(s => s.Group)
            .Include(s => s.Teacher)
            .Include(s => s.Room)
            .Where(s => s.OrganizationId == organizationId)
            .OrderByDescending(s => s.UpdatedDate)
            .Select(s => new LatestScheduleUpdateDto
            {
                ScheduleId = s.Id,
                GroupName = s.Group.Name,
                SubjectName = s.Subject.Name,
                TeacherName = s.Teacher.Name,
                RoomName = s.Room.Name,
                DayOfWeek = s.DayOfWeek.ToString(),
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                UpdatedDate = s.UpdatedDate,
                UpdateType = "Modified"
            })
            .FirstOrDefaultAsync();

        return latestUpdate;
    }

    public async Task<GroupAttendanceDto> GetGroupAttendanceAsync(Guid groupId, DashboardFilterDto? filter = null)
    {
        var group = await dbContext.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            return new GroupAttendanceDto();
        }

        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        
        var totalAttendances = await dbContext.Attendances
            .Include(a => a.Lesson)
            .Where(a => a.Lesson.GroupId == groupId && a.Date >= thirtyDaysAgo)
            .CountAsync();

        var presentAttendances = await dbContext.Attendances
            .Include(a => a.Lesson)
            .Where(a => a.Lesson.GroupId == groupId && a.Date >= thirtyDaysAgo && a.Status == AttendanceStatus.Attend)
            .CountAsync();

        var attendanceRate = totalAttendances > 0 
            ? (decimal)presentAttendances / totalAttendances * 100 
            : 0;

        var activeStudents = await dbContext.Attendances
            .Include(a => a.Lesson)
            .Where(a => a.Lesson.GroupId == groupId && a.Date >= thirtyDaysAgo)
            .Select(a => a.StudentId)
            .Distinct()
            .CountAsync();

        return new GroupAttendanceDto
        {
            GroupId = group.Id,
            GroupName = group.Name,
            GroupCode = group.Code,
            TotalStudents = group.Students.Count,
            ActiveStudents = activeStudents,
            AttendanceRate = attendanceRate
        };
    }
}