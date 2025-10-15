using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Dashboard.Models;
using Trackademy.Application.Persistance;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Dashboard;

/// <summary>
/// Упрощенный сервис для работы с дашбордом - только 2 основных метода
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly TrackademyDbContext dbContext;

    public DashboardService(TrackademyDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <summary>
    /// 📊 Получить краткую сводку дашборда - основные метрики
    /// </summary>
    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(DashboardFilterDto? filter = null)
    {
        var organizationId = GetOrganizationIdFromFilter(filter);

        // Получаем базовые метрики
        var studentCounts = await GetBasicStudentCountsAsync(organizationId);
        var groupCounts = await GetBasicGroupCountsAsync(organizationId);
        var todayLessons = await GetTodayLessonsCountAsync(organizationId);
        var weeklyLessons = await GetWeeklyLessonsCountAsync(organizationId);
        var attendanceRate = await GetBasicAttendanceRateAsync(organizationId);

        return new DashboardSummaryDto
        {
            TotalStudents = studentCounts.Total,
            ActiveStudents = studentCounts.Active,
            TotalGroups = groupCounts.Total,
            ActiveGroups = groupCounts.Active,
            LessonsToday = todayLessons,
            CompletedLessonsToday = weeklyLessons,
            AverageAttendanceRate = attendanceRate,
            LowPerformanceGroupsCount = 3, // Заглушка
            UnpaidStudentsCount = 12, // Заглушка
            TrialStudentsCount = 8, // Заглушка
            TotalDebt = 15000.0m, // Заглушка
            LastUpdated = DateTime.Now
        };
    }

    /// <summary>
    /// 📈 Получить детальный отчет дашборда - расширенная информация
    /// </summary>
    public async Task<DashboardDetailedDto> GetDashboardDetailedAsync(DashboardFilterDto? filter = null)
    {
        var organizationId = GetOrganizationIdFromFilter(filter);

        // Получаем детальные данные
        var studentStats = await GetDetailedStudentStatsAsync(organizationId);
        var groupStats = await GetDetailedGroupStatsAsync(organizationId);
        var lessonStats = await GetDetailedLessonStatsAsync(organizationId);
        var attendanceStats = await GetDetailedAttendanceStatsAsync(organizationId);

        return new DashboardDetailedDto
        {
            StudentStats = studentStats,
            GroupStats = groupStats,
            LessonStats = lessonStats,
            AttendanceStats = attendanceStats,
            LowPerformanceGroups = new List<LowPerformanceGroupDto>(),
            UnpaidStudents = new List<UnpaidStudentDto>(),
            TrialStudents = new List<TrialStudentDto>(),
            TopTeachers = new List<TopTeacherDto>(),
            LatestScheduleUpdate = null,
            GroupAttendanceRates = new List<GroupAttendanceDto>(),
            GeneratedAt = DateTime.Now,
            ReportPeriod = GetReportPeriod(filter)
        };
    }

    private async Task<(int Total, int Active)> GetBasicStudentCountsAsync(Guid organizationId)
    {
        var total = await dbContext.Users
            .Where(u => u.OrganizationId == organizationId && u.Role == RoleEnum.Student)
            .CountAsync();

        var active = await dbContext.Users
            .Where(u => u.OrganizationId == organizationId && u.Role == RoleEnum.Student)
            .CountAsync();

        return (total, active);
    }

    private async Task<(int Total, int Active)> GetBasicGroupCountsAsync(Guid organizationId)
    {
        var total = await dbContext.Groups
            .Where(g => g.OrganizationId == organizationId)
            .CountAsync();

        var active = await dbContext.Groups
            .Where(g => g.OrganizationId == organizationId)
            .CountAsync();

        return (total, active);
    }

    private async Task<int> GetTodayLessonsCountAsync(Guid organizationId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await dbContext.Lessons
            .Where(l => l.Group.OrganizationId == organizationId && l.Date == today)
            .CountAsync();
    }

    private async Task<int> GetWeeklyLessonsCountAsync(Guid organizationId)
    {
        var weekStart = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek));
        var weekEnd = weekStart.AddDays(7);
        return await dbContext.Lessons
            .Where(l => l.Group.OrganizationId == organizationId && 
                       l.Date >= weekStart && l.Date < weekEnd)
            .CountAsync();
    }

    private async Task<decimal> GetBasicAttendanceRateAsync(Guid organizationId)
    {
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        
        var totalAttendances = await dbContext.Attendances
            .Where(a => a.Lesson.Group.OrganizationId == organizationId && a.Date >= thirtyDaysAgo)
            .CountAsync();

        if (totalAttendances == 0) return 0;

        var presentAttendances = await dbContext.Attendances
            .Where(a => a.Lesson.Group.OrganizationId == organizationId && 
                       a.Date >= thirtyDaysAgo && 
                       a.Status == AttendanceStatus.Attend)
            .CountAsync();

        return Math.Round((decimal)presentAttendances * 100 / totalAttendances, 1);
    }

    private async Task<StudentStatsDto> GetDetailedStudentStatsAsync(Guid organizationId)
    {
        var totalStudents = await dbContext.Users
            .Where(u => u.OrganizationId == organizationId && u.Role == RoleEnum.Student)
            .CountAsync();

        var activeStudents = await dbContext.Users
            .Where(u => u.OrganizationId == organizationId && u.Role == RoleEnum.Student)
            .CountAsync();

        var newStudentsThisMonth = await dbContext.Users
            .Where(u => u.OrganizationId == organizationId && 
                       u.Role == RoleEnum.Student && 
                       u.CreatedDate >= DateTime.Now.AddMonths(-1))
            .CountAsync();

        return new StudentStatsDto
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            NewStudentsThisMonth = newStudentsThisMonth
        };
    }

    private async Task<GroupStatsDto> GetDetailedGroupStatsAsync(Guid organizationId)
    {
        var totalGroups = await dbContext.Groups
            .Where(g => g.OrganizationId == organizationId)
            .CountAsync();

        var activeGroups = await dbContext.Groups
            .Where(g => g.OrganizationId == organizationId)
            .CountAsync();

        return new GroupStatsDto
        {
            TotalGroups = totalGroups,
            ActiveGroups = activeGroups,
            AverageGroupSize = 8.5m
        };
    }

    private async Task<LessonStatsDto> GetDetailedLessonStatsAsync(Guid organizationId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = DateOnly.FromDateTime(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));

        var todayLessons = await dbContext.Lessons
            .Where(l => l.Group.OrganizationId == organizationId && l.Date == today)
            .CountAsync();

        var monthlyLessons = await dbContext.Lessons
            .Where(l => l.Group.OrganizationId == organizationId && l.Date >= monthStart)
            .CountAsync();

        return new LessonStatsDto
        {
            LessonsToday = todayLessons,
            CompletedLessonsToday = 89, // Заглушка
            CancelledLessonsToday = 3, // Заглушка
            LessonsThisMonth = monthlyLessons
        };
    }

    private async Task<AttendanceStatsDto> GetDetailedAttendanceStatsAsync(Guid organizationId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));

        var query = dbContext.Attendances
            .Where(a => a.Lesson.Group.OrganizationId == organizationId);

        var totalAttendances = await query
            .Where(a => a.Date >= thirtyDaysAgo)
            .CountAsync();

        var presentAttendances = await query
            .Where(a => a.Date >= thirtyDaysAgo && a.Status == AttendanceStatus.Attend)
            .CountAsync();

        var overallAttendanceRate = totalAttendances > 0 
            ? Math.Round((decimal)presentAttendances * 100 / totalAttendances, 1) 
            : 0;

        var presentToday = await query
            .Where(a => a.Date == today && a.Status == AttendanceStatus.Attend)
            .CountAsync();

        var absentToday = await query
            .Where(a => a.Date == today && a.Status == AttendanceStatus.NotAttend)
            .CountAsync();

        var lateToday = await query
            .Where(a => a.Date == today && a.Status == AttendanceStatus.Late)
            .CountAsync();

        return new AttendanceStatsDto
        {
            OverallAttendanceRate = overallAttendanceRate,
            PresentStudentsToday = presentToday,
            AbsentStudentsToday = absentToday,
            LateStudentsToday = lateToday,
            GroupAttendanceRates = new List<GroupAttendanceSummaryDto>()
        };
    }

    private Guid GetOrganizationIdFromFilter(DashboardFilterDto? filter)
    {
        if (filter?.OrganizationId == null)
        {
            throw new ArgumentException("OrganizationId is required in filter");
        }
        return filter.OrganizationId.Value;
    }

    private string GetReportPeriod(DashboardFilterDto? filter)
    {
        if (filter?.StartDate != null && filter?.EndDate != null)
        {
            return $"С {filter.StartDate:dd.MM.yyyy} по {filter.EndDate:dd.MM.yyyy}";
        }
        return "За последние 30 дней";
    }
}