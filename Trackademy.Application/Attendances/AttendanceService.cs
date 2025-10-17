using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Attendances.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Services;
using Trackademy.Application.Shared.Exception;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Attendances;

public class AttendanceService : IAttendanceService
{
    private readonly TrackademyDbContext _context;
    private readonly IMapper _mapper;
    private readonly IExcelExportService _excelExportService;

    public AttendanceService(TrackademyDbContext context, IMapper mapper, IExcelExportService excelExportService)
    {
        _context = context;
        _mapper = mapper;
        _excelExportService = excelExportService;
    }

    public async Task<bool> MarkAttendancesAsync(AttendanceBulkCreateModel model)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Group)
            .ThenInclude(g => g.Students)
            .FirstOrDefaultAsync(l => l.Id == model.LessonId);

        if (lesson == null)
            throw new ConflictException("Урок не найден");

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Almaty");
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        
        if (lesson.Date > today)
        {
            throw new ConflictException("Нельзя отмечать посещаемость для будущих уроков.");
        }

        // Валидация: проверяем, что все пользователи являются студентами
        var userIds = model.Attendances.Select(a => a.StudentId).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var nonStudents = users.Where(u => u.Role != RoleEnum.Student).ToList();
        if (nonStudents.Any())
        {
            var nonStudentNames = string.Join(", ", nonStudents.Select(u => u.FullName));
            throw new ConflictException($"Нельзя отмечать посещаемость для учителей или других ролей: {nonStudentNames}");
        }

        // Проверяем, что все студенты принадлежат к группе урока
        var groupStudentIds = lesson.Group.Students.Select(s => s.Id).ToHashSet();
        var notInGroup = userIds.Where(id => !groupStudentIds.Contains(id)).ToList();
        if (notInGroup.Any())
        {
            throw new ConflictException("Некоторые студенты не принадлежат к группе данного урока");
        }

        var currentDate = DateOnly.FromDateTime(DateTime.Today);

        var existingAttendances = await _context.Attendances
            .Where(a => a.LessonId == model.LessonId)
            .ToListAsync();

        foreach (var attendanceRecord in model.Attendances)
        {
            var existing = existingAttendances
                .FirstOrDefault(a => a.StudentId == attendanceRecord.StudentId);

            if (existing != null)
            {
                existing.Status = attendanceRecord.Status;
                existing.Date = currentDate;
            }
            else
            {
                var attendance = new Attendance
                {
                    Id = Guid.NewGuid(),
                    StudentId = attendanceRecord.StudentId,
                    LessonId = model.LessonId,
                    Date = currentDate,
                    Status = attendanceRecord.Status
                };

                await _context.Attendances.AddAsync(attendance);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AttendanceDto?> UpdateAttendanceAsync(AttendanceUpdateModel model)
    {
        var attendance = await _context.Attendances
            .Include(a => a.Student)
            .Include(a => a.Lesson)
            .ThenInclude(l => l.Group)
            .ThenInclude(g => g.Subject)
            .Include(a => a.Lesson.Teacher)
            .FirstOrDefaultAsync(a => a.LessonId == model.LessonId && a.StudentId == model.StudentId);

        if (attendance == null)
            throw new ConflictException("Запись о посещаемости не найдена");

        attendance.Status = model.Status;
        await _context.SaveChangesAsync();

        return _mapper.Map<AttendanceDto>(attendance);
    }

    public async Task<AttendanceDto?> GetAttendanceByIdAsync(Guid id)
    {
        var attendance = await _context.Attendances
            .Include(a => a.Student)
            .Include(a => a.Lesson)
            .ThenInclude(l => l.Group)
            .ThenInclude(g => g.Subject)
            .Include(a => a.Lesson.Teacher)
            .FirstOrDefaultAsync(a => a.Id == id);

        return attendance == null ? null : _mapper.Map<AttendanceDto>(attendance);
    }

    public async Task<PagedResult<AttendanceDto>> GetAttendancesAsync(AttendanceFilterModel filter)
    {
        var organizationExists = await _context.Organizations
            .AnyAsync(o => o.Id == filter.OrganizationId);
        if (!organizationExists)
        {
            throw new ConflictException("Организация не найдена");
        }

        var query = _context.Attendances
            .AsNoTracking()
            .Where(a => a.Student.OrganizationId == filter.OrganizationId)
            .Include(a => a.Student)
            .Include(a => a.Lesson)
            .ThenInclude(l => l.Group)
            .ThenInclude(g => g.Subject)
            .Include(a => a.Lesson.Teacher)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.StudentSearch))
        {
            var searchTerm = filter.StudentSearch.ToLower();
            query = query.Where(a => 
                a.Student.FullName.ToLower().Contains(searchTerm) ||
                a.Student.Login.ToLower().Contains(searchTerm));
        }

        if (filter.GroupId.HasValue)
        {
            query = query.Where(a => a.Lesson.GroupId == filter.GroupId.Value);
        }

        if (filter.SubjectId.HasValue)
        {
            query = query.Where(a => a.Lesson.Group.SubjectId == filter.SubjectId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(a => a.Status == filter.Status.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(a => a.Date >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(a => a.Date <= filter.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Student.FullName)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var mappedItems = _mapper.Map<List<AttendanceDto>>(items);

        return new PagedResult<AttendanceDto>
        {
            Items = mappedItems,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<AttendanceStatsDto> GetStudentAttendanceStatsAsync(Guid studentId, DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == studentId);
        if (student == null)
            throw new ConflictException("Студент не найден");

        var query = _context.Attendances
            .Where(a => a.StudentId == studentId);

        if (fromDate.HasValue)
            query = query.Where(a => a.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Date <= toDate.Value);

        var attendances = await query.ToListAsync();

        var totalLessons = attendances.Count;
        var attendedLessons = attendances.Count(a => a.Status == AttendanceStatus.Attend);
        var missedLessons = attendances.Count(a => a.Status == AttendanceStatus.NotAttend);
        var lateLessons = attendances.Count(a => a.Status == AttendanceStatus.Late);
        var specialReasonLessons = attendances.Count(a => a.Status == AttendanceStatus.SpecialReason);

        var attendancePercentage = totalLessons > 0 
            ? Math.Round((double)attendedLessons / totalLessons * 100, 0)
            : 0;

        return new AttendanceStatsDto
        {
            StudentId = studentId,
            StudentName = student.FullName,
            TotalLessons = totalLessons,
            AttendedLessons = attendedLessons,
            MissedLessons = missedLessons,
            LateLessons = lateLessons,
            SpecialReasonLessons = specialReasonLessons,
            AttendancePercentage = attendancePercentage
        };
    }

    public async Task<List<AttendanceReportDto>> GetGroupAttendanceReportAsync(Guid groupId, DateOnly fromDate, DateOnly toDate)
    {
        var group = await _context.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new ConflictException("Группа не найдена");

        var attendances = await _context.Attendances
            .Include(a => a.Student)
            .Include(a => a.Lesson)
            .ThenInclude(l => l.Group)
            .ThenInclude(g => g.Subject)
            .Where(a => a.Lesson.GroupId == groupId && 
                       a.Date >= fromDate && 
                       a.Date <= toDate)
            .OrderBy(a => a.Student.FullName)
            .ThenBy(a => a.Date)
            .ToListAsync();

        var result = attendances
            .GroupBy(a => a.StudentId)
            .Select(g => new AttendanceReportDto
            {
                StudentId = g.Key,
                StudentName = g.First().Student.FullName,
                StudentLogin = g.First().Student.Login,
                Lessons = g.Select(a => new LessonAttendanceDto
                {
                    LessonId = a.LessonId,
                    Date = a.Date,
                    SubjectName = a.Lesson.Group.Subject.Name,
                    StartTime = a.Lesson.StartTime,
                    EndTime = a.Lesson.EndTime,
                    Status = a.Status,
                    StatusName = GetStatusName(a.Status)
                }).ToList()
            })
            .ToList();

        return result;
    }

    public async Task<byte[]> ExportAttendanceReportAsync(AttendanceExportFilterModel filter)
    {
        if (filter.FromDate > filter.ToDate)
        {
            throw new ConflictException("Дата начала не может быть больше даты окончания");
        }

        // Валидация организации (обязательное поле)
        var organizationExists = await _context.Organizations
            .AnyAsync(o => o.Id == filter.OrganizationId);
        if (!organizationExists)
        {
            throw new ConflictException("Организация не найдена");
        }

        // Валидация группы с проверкой принадлежности к организации
        if (filter.GroupId.HasValue)
        {
            var groupExists = await _context.Groups
                .AnyAsync(g => g.Id == filter.GroupId.Value && g.OrganizationId == filter.OrganizationId);
            if (!groupExists)
            {
                throw new ConflictException("Группа не найдена в указанной организации");
            }
        }

        // Валидация предмета с проверкой принадлежности к организации
        if (filter.SubjectId.HasValue)
        {
            var subjectExists = await _context.Subjects
                .AnyAsync(s => s.Id == filter.SubjectId.Value && s.OrganizationId == filter.OrganizationId);
            if (!subjectExists)
            {
                throw new ConflictException("Предмет не найден в указанной организации");
            }
        }

        // Валидация студента с проверкой принадлежности к организации
        if (filter.StudentId.HasValue)
        {
            var studentExists = await _context.Users
                .AnyAsync(u => u.Id == filter.StudentId.Value && 
                             u.Role == RoleEnum.Student && 
                             u.OrganizationId == filter.OrganizationId);
            if (!studentExists)
            {
                throw new ConflictException("Студент не найден в указанной организации");
            }
        }

        var attendanceFilter = new AttendanceFilterModel
        {
            OrganizationId = filter.OrganizationId,
            GroupId = filter.GroupId,
            SubjectId = filter.SubjectId,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Status = filter.Status,
            PageNumber = 1,
            PageSize = int.MaxValue
        };

        var attendanceData = await GetAttendancesAsync(attendanceFilter);
        
        if (filter.StudentId.HasValue)
        {
            attendanceData.Items = attendanceData.Items
                .Where(a => a.StudentId == filter.StudentId.Value)
                .ToList();
        }

        if (!attendanceData.Items.Any())
        {
            throw new ConflictException("Нет данных для экспорта за указанный период с выбранными фильтрами");
        }

        return await _excelExportService.ExportAttendanceReportAsync(attendanceData.Items, filter);
    }

    private static string GetStatusName(AttendanceStatus status)
    {
        return status switch
        {
            AttendanceStatus.Attend => "Присутствовал",
            AttendanceStatus.NotAttend => "Отсутствовал",
            AttendanceStatus.Late => "Опоздал",
            AttendanceStatus.SpecialReason => "Уважительная причина",
            _ => "Неизвестно"
        };
    }
}