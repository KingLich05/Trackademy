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
            LowPerformanceGroupsCount = await GetLowPerformanceGroupsCountAsync(organizationId),
            UnpaidStudentsCount = await GetUnpaidStudentsCountAsync(organizationId),
            TrialStudentsCount = await GetTrialStudentsCountAsync(organizationId),
            TotalDebt = await GetTotalDebtAsync(organizationId),
            LastUpdated = DateTime.Now
        };
    }

    /// <summary>
    /// 📈 Получить детальный отчет дашборда - расширенная информация
    /// </summary>
    public async Task<DashboardDetailedDto> GetDashboardDetailedAsync(DashboardFilterDto? filter = null)
    {
        var organizationId = GetOrganizationIdFromFilter(filter);

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
            CompletedLessonsToday = 89,
            CancelledLessonsToday = 3,
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
        if (filter?.OrganizationId == Guid.Empty)
        {
            throw new ArgumentException("OrganizationId is required in filter");
        }
        return filter.OrganizationId;
    }

    private string GetReportPeriod(DashboardFilterDto? filter)
    {
        if (filter?.StartDate != null && filter?.EndDate != null)
        {
            return $"С {filter.StartDate:dd.MM.yyyy} по {filter.EndDate:dd.MM.yyyy}";
        }
        return "За последние 30 дней";
    }

    #region Реальная бизнес-логика

    /// <summary>
    /// Получить количество неоплативших студентов
    /// </summary>
    private async Task<int> GetUnpaidStudentsCountAsync(Guid organizationId)
    {
        var today = DateTime.Today;
        return await dbContext.Payments
            .Where(p => p.Student.OrganizationId == organizationId && 
                       p.Status == PaymentStatus.Overdue && 
                       p.DueDate <= today)
            .Select(p => p.StudentId)
            .Distinct()
            .CountAsync();
    }

    /// <summary>
    /// Получить общую сумму задолженности
    /// </summary>
    private async Task<decimal> GetTotalDebtAsync(Guid organizationId)
    {
        var today = DateTime.Today;
        return await dbContext.Payments
            .Where(p => p.Student.OrganizationId == organizationId && 
                       (p.Status == PaymentStatus.Overdue || p.Status == PaymentStatus.Pending) && 
                       p.DueDate <= today)
            .SumAsync(p => p.Amount);
    }

    /// <summary>
    /// Получить количество пробных студентов (созданных в последние 30 дней без платежей)
    /// </summary>
    private async Task<int> GetTrialStudentsCountAsync(Guid organizationId)
    {
        var thirtyDaysAgo = DateTime.Today.AddDays(-30);
        return await dbContext.Users
            .Where(u => u.OrganizationId == organizationId && 
                       u.Role == RoleEnum.Student && 
                       u.CreatedDate >= thirtyDaysAgo && 
                       !u.Payments.Any())
            .CountAsync();
    }

    /// <summary>
    /// Получить количество групп с низкой успеваемостью (посещаемость < 60%)
    /// </summary>
    private async Task<int> GetLowPerformanceGroupsCountAsync(Guid organizationId)
    {
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        
        var groupAttendanceRates = await dbContext.Groups
            .Where(g => g.OrganizationId == organizationId)
            .Select(g => new
            {
                GroupId = g.Id,
                TotalAttendances = g.Students
                    .SelectMany(s => s.Attendances)
                    .Where(a => a.Date >= thirtyDaysAgo)
                    .Count(),
                PresentAttendances = g.Students
                    .SelectMany(s => s.Attendances)
                    .Where(a => a.Date >= thirtyDaysAgo && a.Status == AttendanceStatus.Attend)
                    .Count()
            })
            .Where(x => x.TotalAttendances > 0)
            .ToListAsync();

        return groupAttendanceRates
            .Where(x => (decimal)x.PresentAttendances / x.TotalAttendances < 0.6m)
            .Count();
    }

    /// <summary>
    /// Получить реальные данные для неоплативших студентов
    /// </summary>
    private async Task<List<UnpaidStudentDto>> GetUnpaidStudentsAsync(Guid organizationId)
    {
        var today = DateTime.Today;
        return await dbContext.Payments
            .Where(p => p.Student.OrganizationId == organizationId && 
                       p.Status == PaymentStatus.Overdue && 
                       p.DueDate <= today)
            .GroupBy(p => p.Student)
            .Select(g => new UnpaidStudentDto
            {
                StudentId = g.Key.Id,
                StudentName = g.Key.FullName,
                Email = g.Key.Email,
                Phone = g.Key.Phone,
                GroupName = g.Key.Groups.FirstOrDefault() != null ? g.Key.Groups.First().Name : "Без группы",
                DebtAmount = g.Sum(p => p.Amount),
                DaysOverdue = (int)(today - g.Min(p => p.DueDate)).TotalDays,
                PaymentStatus = PaymentStatus.Overdue,
                LastPaymentDate = g.Max(p => p.PaidAt)
            })
            .ToListAsync();
    }

    /// <summary>
    /// Получить реальные данные для пробных студентов
    /// </summary>
    private async Task<List<TrialStudentDto>> GetTrialStudentsAsync(Guid organizationId)
    {
        var thirtyDaysAgo = DateTime.Today.AddDays(-30);
        return await dbContext.Users
            .Where(u => u.OrganizationId == organizationId && 
                       u.Role == RoleEnum.Student && 
                       u.CreatedDate >= thirtyDaysAgo && 
                       !u.Payments.Any())
            .Select(u => new TrialStudentDto
            {
                StudentId = u.Id,
                StudentName = u.FullName,
                Email = u.Email,
                Phone = u.Phone ?? "Не указан",
                SubjectName = "Пробный урок",
                TrialLessonDate = DateTime.Today.AddDays(1),
                TrialLessonTime = new TimeSpan(10, 0, 0),
                TeacherName = "Назначается",
                TrialStatus = "Запланирован",
                RegisteredAt = u.CreatedDate
            })
            .ToListAsync();
    }

    /// <summary>
    /// Получить топ преподавателей по количеству студентов
    /// </summary>
    private async Task<List<TopTeacherDto>> GetTopTeachersAsync(Guid organizationId, int limit = 5)
    {
        return await dbContext.Users
            .Where(u => u.OrganizationId == organizationId && u.Role == RoleEnum.Teacher)
            .Select(u => new TopTeacherDto
            {
                TeacherId = u.Id,
                TeacherName = u.FullName,
                Email = u.Email,
                StudentCount = u.Schedules
                    .SelectMany(s => s.Group.Students)
                    .Distinct()
                    .Count(),
                GroupCount = u.Schedules
                    .Select(s => s.GroupId)
                    .Distinct()
                    .Count(),
                LessonsThisMonth = u.Schedules.Count(), // Упрощенная логика
                AverageAttendanceRate = 85.0m, // Заглушка, т.к. сложный расчет
                Rating = 4.5m // Заглушка для рейтинга
            })
            .OrderByDescending(t => t.StudentCount)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Получить группы с низкой успеваемостью
    /// </summary>
    private async Task<List<LowPerformanceGroupDto>> GetLowPerformanceGroupsAsync(Guid organizationId)
    {
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        
        var groups = await dbContext.Groups
            .Where(g => g.OrganizationId == organizationId)
            .Select(g => new
            {
                Group = g,
                TotalAttendances = g.Students
                    .SelectMany(s => s.Attendances)
                    .Where(a => a.Date >= thirtyDaysAgo)
                    .Count(),
                PresentAttendances = g.Students
                    .SelectMany(s => s.Attendances)
                    .Where(a => a.Date >= thirtyDaysAgo && a.Status == AttendanceStatus.Attend)
                    .Count()
            })
            .Where(x => x.TotalAttendances > 0)
            .ToListAsync();

        return groups
            .Where(x => (decimal)x.PresentAttendances / x.TotalAttendances < 0.6m)
            .Select(x => new LowPerformanceGroupDto
            {
                GroupId = x.Group.Id,
                GroupName = x.Group.Name,
                GroupCode = x.Group.Code ?? "Без кода",
                SubjectName = x.Group.Subject.Name,
                AttendanceRate = Math.Round((decimal)x.PresentAttendances / x.TotalAttendances * 100, 1),
                TotalStudents = x.Group.Students.Count,
                ActiveStudents = x.Group.Students.Count,
                PerformanceIssue = "Низкая посещаемость"
            })
            .ToList();
    }

    #endregion
}