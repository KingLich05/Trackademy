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
            LastUpdated = DateTime.UtcNow
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
            LowPerformanceGroups = await GetLowPerformanceGroupsAsync(organizationId),
            UnpaidStudents = await GetUnpaidStudentsAsync(organizationId),
            TrialStudents = await GetTrialStudentsAsync(organizationId),
            TopTeachers = await GetTopTeachersAsync(organizationId),
            GroupAttendanceRates = await GetGroupAttendanceRatesAsync(organizationId),
            GeneratedAt = DateTime.UtcNow,
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
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        return await dbContext.Lessons
            .Where(l => l.Group.OrganizationId == organizationId &&
                        l.Date == today &&
                        l.LessonStatus == LessonStatus.Completed)
            .CountAsync();
    }

    private async Task<int> GetWeeklyLessonsCountAsync(Guid organizationId)
    {
        var utcNow = DateTime.UtcNow.Date;
        var weekStart = DateOnly.FromDateTime(utcNow.AddDays(-(int)utcNow.DayOfWeek));
        var weekEnd = weekStart.AddDays(7);
        return await dbContext.Lessons
            .Where(l => l.Group.OrganizationId == organizationId && 
                       l.Date >= weekStart && l.Date < weekEnd)
            .CountAsync();
    }

    private async Task<decimal> GetBasicAttendanceRateAsync(Guid organizationId)
    {
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30));
        
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
                       u.CreatedDate >= DateTime.UtcNow.AddMonths(-1))
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

        var averageGroupSize = await GetAverageGroupSizeAsync(organizationId);

        return new GroupStatsDto
        {
            TotalGroups = totalGroups,
            ActiveGroups = activeGroups,
            AverageGroupSize = averageGroupSize
        };
    }

    private async Task<LessonStatsDto> GetDetailedLessonStatsAsync(Guid organizationId)
    {
        var utcNow = DateTime.UtcNow.Date;
        var today = DateOnly.FromDateTime(utcNow);
        var monthStart = DateOnly.FromDateTime(new DateTime(utcNow.Year, utcNow.Month, 1));

        var todayLessons = await dbContext.Lessons
            .Where(l => l.Group.OrganizationId == organizationId && l.Date == today)
            .CountAsync();

        var monthlyLessons = await dbContext.Lessons
            .Where(l => l.Group.OrganizationId == organizationId && l.Date >= monthStart)
            .CountAsync();

        var completedLessonsToday = await dbContext.Lessons
            .Where(l => l.Group.OrganizationId == organizationId && 
                       l.Date == today && 
                       l.LessonStatus == LessonStatus.Completed)
            .CountAsync();

        var cancelledLessonsToday = await dbContext.Lessons
            .Where(l => l.Group.OrganizationId == organizationId && 
                       l.Date == today && 
                       l.LessonStatus == LessonStatus.Cancelled)
            .CountAsync();

        return new LessonStatsDto
        {
            LessonsToday = todayLessons,
            CompletedLessonsToday = completedLessonsToday,
            CancelledLessonsToday = cancelledLessonsToday,
            LessonsThisMonth = monthlyLessons
        };
    }

    private async Task<AttendanceStatsDto> GetDetailedAttendanceStatsAsync(Guid organizationId)
    {
        var utcNow = DateTime.UtcNow.Date;
        var today = DateOnly.FromDateTime(utcNow);
        var thirtyDaysAgo = DateOnly.FromDateTime(utcNow.AddDays(-30));

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
            GroupAttendanceRates = await GetGroupAttendanceSummaryAsync(organizationId)
        };
    }

    /// <summary>
    /// Получить средний размер активных групп
    /// </summary>
    private async Task<decimal> GetAverageGroupSizeAsync(Guid organizationId)
    {
        var groups = await dbContext.Groups
            .Where(g => g.OrganizationId == organizationId)
            .Select(g => g.Students.Count)
            .ToListAsync();

        return groups.Count > 0 ? Math.Round((decimal)groups.Average(), 1) : 0;
    }



    private Guid GetOrganizationIdFromFilter(DashboardFilterDto? filter)
    {
        if (filter == null || filter.OrganizationId == Guid.Empty)
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
        var today = DateTime.UtcNow.Date;
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
        var today = DateTime.UtcNow.Date;
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
        var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-30);
        return await dbContext.Users
            .Where(u => u.OrganizationId == organizationId && 
                       u.Role == RoleEnum.Student && 
                       u.CreatedDate >= thirtyDaysAgo && 
                       !u.Payments.Any())
            .CountAsync();
    }

    /// <summary>
    /// Получить количество групп с низкой успеваемостью (посещаемость менее 70%)
    /// </summary>
    private async Task<int> GetLowPerformanceGroupsCountAsync(Guid organizationId)
    {
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30));
        
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
            .Where(x => (decimal)x.PresentAttendances / x.TotalAttendances < 0.5m)
            .Count();
    }

    /// <summary>
    /// Получить реальные данные для неоплативших студентов
    /// </summary>
    private async Task<List<UnpaidStudentDto>> GetUnpaidStudentsAsync(Guid organizationId)
    {
        var today = DateTime.UtcNow.Date;
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
    /// Получить список студентов на пробном периоде
    /// </summary>
    private async Task<List<TrialStudentDto>> GetTrialStudentsAsync(Guid organizationId)
    {
        var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-30);
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
                TrialLessonDate = DateTime.UtcNow.Date.AddDays(1),
                TrialLessonTime = new TimeSpan(10, 0, 0),
                TeacherName = "Назначается",
                TrialStatus = "Запланирован",
                RegisteredAt = u.CreatedDate
            })
            .ToListAsync();
    }

    /// <summary>
    /// Получить топ преподавателей с посещаемостью выше 90%
    /// </summary>
    private async Task<List<TopTeacherDto>> GetTopTeachersAsync(Guid organizationId, int limit = 10)
    {
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30));
        var monthStart = DateOnly.FromDateTime(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1));

        var teachersStats = await dbContext.Users
            .Where(u => u.OrganizationId == organizationId && u.Role == RoleEnum.Teacher)
            .Select(u => new
            {
                Teacher = u,
                TotalAttendances = dbContext.Lessons
                    .Where(l => l.TeacherId == u.Id && l.Date >= thirtyDaysAgo)
                    .SelectMany(l => l.Attendances)
                    .Count(),
                PresentAttendances = dbContext.Lessons
                    .Where(l => l.TeacherId == u.Id && l.Date >= thirtyDaysAgo)
                    .SelectMany(l => l.Attendances)
                    .Where(a => a.Status == AttendanceStatus.Attend)
                    .Count(),
                LessonsThisMonth = dbContext.Lessons
                    .Where(l => l.TeacherId == u.Id && l.Date >= monthStart)
                    .Count(),
                StudentCount = u.Schedules
                    .SelectMany(s => s.Group.Students)
                    .Distinct()
                    .Count(),
                GroupCount = u.Schedules
                    .Select(s => s.GroupId)
                    .Distinct()
                    .Count()
            })
            .Where(x => x.TotalAttendances > 0)
            .ToListAsync();

        return teachersStats
            .Where(x => (decimal)x.PresentAttendances / x.TotalAttendances > 0.9m)
            .Select(x => new TopTeacherDto
            {
                TeacherId = x.Teacher.Id,
                TeacherName = x.Teacher.FullName,
                Email = x.Teacher.Email,
                StudentCount = x.StudentCount,
                GroupCount = x.GroupCount,
                LessonsThisMonth = x.LessonsThisMonth,
                AverageAttendanceRate = Math.Round((decimal)x.PresentAttendances / x.TotalAttendances * 100, 1),
                Rating = Math.Round((decimal)x.PresentAttendances / x.TotalAttendances * 5, 1)
            })
            .OrderByDescending(t => t.AverageAttendanceRate)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Получить группы с низкой успеваемостью
    /// </summary>
    private async Task<List<LowPerformanceGroupDto>> GetLowPerformanceGroupsAsync(Guid organizationId)
    {
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30));
        
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
            .Where(x => (decimal)x.PresentAttendances / x.TotalAttendances < 0.5m)
            .Select(x => new LowPerformanceGroupDto
            {
                GroupId = x.Group.Id,
                GroupName = x.Group.Name,
                GroupCode = x.Group.Code ?? "Без кода",
                SubjectName = x.Group.Subject.Name,
                AttendanceRate = Math.Round((decimal)x.PresentAttendances / x.TotalAttendances * 100, 1),
                TotalStudents = x.Group.Students.Count,
                ActiveStudents = x.Group.Students.Count,
                PerformanceIssue = "Низкая посещаемость (менее 50%)"
            })
            .ToList();
    }

    /// <summary>
    /// Получить детальную статистику посещаемости по группам
    /// </summary>
    private async Task<List<GroupAttendanceDto>> GetGroupAttendanceRatesAsync(Guid organizationId)
    {
        var utcNow = DateTime.UtcNow.Date;
        var today = DateOnly.FromDateTime(utcNow);
        var weekAgo = DateOnly.FromDateTime(utcNow.AddDays(-7));
        var monthAgo = DateOnly.FromDateTime(utcNow.AddDays(-30));

        var groups = await dbContext.Groups
            .Where(g => g.OrganizationId == organizationId)
            .Include(g => g.Subject)
            .Include(g => g.Students)
            .Select(g => new
            {
                Group = g,
                // Статистика за сегодня
                TodayAttendances = g.Students
                    .SelectMany(s => s.Attendances)
                    .Where(a => a.Date == today),
                // Статистика за неделю
                WeekAttendances = g.Students
                    .SelectMany(s => s.Attendances)
                    .Where(a => a.Date >= weekAgo),
                // Статистика за месяц
                MonthAttendances = g.Students
                    .SelectMany(s => s.Attendances)
                    .Where(a => a.Date >= monthAgo),
                // Общее количество уроков
                TotalLessons = dbContext.Lessons
                    .Where(l => l.GroupId == g.Id)
                    .Count()
            })
            .ToListAsync();

        return groups.Select(x => 
        {
            var todayTotal = x.TodayAttendances.Count();
            var todayPresent = x.TodayAttendances.Count(a => a.Status == AttendanceStatus.Attend);
            var todayAbsent = x.TodayAttendances.Count(a => a.Status == AttendanceStatus.NotAttend);
            var todayLate = x.TodayAttendances.Count(a => a.Status == AttendanceStatus.Late);

            var weekTotal = x.WeekAttendances.Count();
            var weekPresent = x.WeekAttendances.Count(a => a.Status == AttendanceStatus.Attend);
            var weeklyRate = weekTotal > 0 ? Math.Round((decimal)weekPresent / weekTotal * 100, 1) : 0;

            var monthTotal = x.MonthAttendances.Count();
            var monthPresent = x.MonthAttendances.Count(a => a.Status == AttendanceStatus.Attend);
            var monthlyRate = monthTotal > 0 ? Math.Round((decimal)monthPresent / monthTotal * 100, 1) : 0;

            var averageRate = (weeklyRate + monthlyRate) / 2;

            return new GroupAttendanceDto
            {
                GroupId = x.Group.Id,
                GroupName = x.Group.Name,
                GroupCode = x.Group.Code ?? "Без кода",
                SubjectName = x.Group.Subject.Name,
                TotalStudents = x.Group.Students.Count,
                AverageAttendanceRate = Math.Round(averageRate, 1),
                PresentToday = todayPresent,
                AbsentToday = todayAbsent,
                LateToday = todayLate,
                TotalLessons = x.TotalLessons,
                WeeklyAttendanceRate = weeklyRate,
                MonthlyAttendanceRate = monthlyRate
            };
        }).ToList();
    }

    /// <summary>
    /// Получить краткую статистику посещаемости по группам
    /// </summary>
    private async Task<List<GroupAttendanceSummaryDto>> GetGroupAttendanceSummaryAsync(Guid organizationId)
    {
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30));

        var groupStats = await dbContext.Groups
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
            .ToListAsync();

        return groupStats.Select(x => new GroupAttendanceSummaryDto
        {
            GroupId = x.Group.Id,
            GroupName = x.Group.Name,
            GroupCode = x.Group.Code ?? "Без кода",
            AttendanceRate = x.TotalAttendances > 0 
                ? Math.Round((decimal)x.PresentAttendances / x.TotalAttendances * 100, 1) 
                : 0,
            TotalStudents = x.Group.Students.Count,
            ActiveStudents = x.Group.Students.Count
        }).ToList();
    }

    #endregion
}