using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.GroupServices.Models;
using Trackademy.Application.PaymentServices;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Exception;
using Trackademy.Application.Shared.Extensions;
using Trackademy.Application.Shared.Models;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.GroupServices;

public class GroupService:
    BaseService<Groups, GroupsDto, GroupsAddModel, GroupsUpdateModel>, IGroupService
{
    private TrackademyDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPaymentService _paymentService;
    
    public GroupService(TrackademyDbContext context, IMapper mapper, IPaymentService paymentService)
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
        _paymentService = paymentService;
    }

    public override async Task<Guid> UpdateAsync(Guid id, GroupsUpdateModel dto)
    {
        var entity = await _context.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (entity is null) return Guid.Empty;
        
        if (!string.IsNullOrWhiteSpace(dto.Code) && 
            !dto.Code.Equals(entity.Code, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _context.Groups.AnyAsync(g =>
                g.OrganizationId == entity.OrganizationId &&
                g.Code.ToLower() == dto.Code.ToLower() &&
                g.Id != entity.Id);

            if (exists)
            {
                throw new ConflictException($"Группа с кодом '{dto.Code}' уже существует в этой организации.");
            }
        }

        _mapper.Map(dto, entity);

        if (dto.StudentIds is not null || dto.StudentIds.Any())
        {
            var desired = dto.StudentIds.Distinct().ToHashSet();
            var current = entity.Students.Select(s => s.Id).ToHashSet();

            var idsToAdd    = desired.Except(current).ToList();
            var idsToRemove = current.Except(desired).ToList();

            // Удаление студентов - отменяем их неоплаченные платежи
            if (idsToRemove.Count > 0)
            {
                var removeSet = idsToRemove.ToHashSet();
                entity.Students = entity.Students
                    .Where(s => !removeSet.Contains(s.Id))
                    .ToList();

                // Отменяем неоплаченные платежи удаленных студентов
                foreach (var studentId in idsToRemove)
                {
                    await _paymentService.CancelStudentPaymentsInGroupAsync(
                        studentId, 
                        entity.Id, 
                        "Студент удален из группы");
                }
            }

            // Добавление новых студентов - создаем им платежи
            if (idsToAdd.Count > 0)
            {
                // Создаем записи GroupStudent для новых студентов
                foreach (var studentId in idsToAdd)
                {
                    var groupStudent = new GroupStudent
                    {
                        Id = Guid.NewGuid(),
                        GroupId = entity.Id,
                        StudentId = studentId,
                        DiscountType = DiscountType.Percentage,
                        DiscountValue = 0,
                        DiscountReason = null,
                        JoinedAt = DateTime.UtcNow
                    };
                    await _context.GroupStudents.AddAsync(groupStudent);
                }
                
                // Сохраняем GroupStudent записи в базу
                await _context.SaveChangesAsync();
                
                // Проверяем активность группы через Schedule
                var isGroupActive = await IsGroupActiveAsync(entity.Id);
                
                // Создаем платежи только если группа активна
                if (isGroupActive)
                {
                    await _paymentService.CreatePaymentsForStudentsAsync(idsToAdd, entity.Id);
                }
            }
        }

        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<List<GroupsDto>> GetAllAsync(Guid organizationId)
    {
        var groups = await _context.Groups
            .Include(x => x.Students)
            .Include(x => x.Subject)
            .Include(x => x.GroupStudents)
                .ThenInclude(gs => gs.Student)
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var groupsDto = groups.Select(group => new GroupsDto
        {
            Id = group.Id,
            Name = group.Name,
            Code = group.Code,
            Level = group.Level,
            Subject = new SubjectMinimalViewModel
            {
                SubjectId = group.Subject.Id,
                SubjectName = group.Subject.Name
            },
            Students = group.GroupStudents.Select(gs => new UserMinimalViewModel
            {
                StudentId = gs.StudentId,
                StudentName = gs.Student.FullName,
                IsFrozen = gs.IsFrozen,
                FrozenFrom = gs.FrozenFrom,
                FrozenTo = gs.FrozenTo,
                FreezeReason = gs.FreezeReason
            }).ToList()
        }).ToList();

        return groupsDto;
    }

    public async Task<Guid> CreateGroup(GroupsAddModel model)
    {
        if (string.IsNullOrEmpty(model.Code) || string.IsNullOrWhiteSpace(model.Code))
        {
            model.Code = GenerateCode();
        }

        var isExists = await _context.Groups.AnyAsync(r =>
            r.OrganizationId == model.OrganizationId &&
            r.Code.ToLower() == model.Code.ToLower());

        if (isExists)
        {
            throw new ConflictException($"Группа с кодом '{model.Code}' уже существует в этой организации.");
        }

        if (string.IsNullOrEmpty(model.Name) || string.IsNullOrWhiteSpace(model.Name))
        {
            var subject = await _context.Subjects.FindAsync(model.SubjectId);
            model.Name = subject?.Name + "-" + model.Code;
        }

        var group = _mapper.Map<Groups>(model);
        await _context.Groups.AddAsync(group);
        await _context.SaveChangesAsync();
        
        // Создаем записи GroupStudent и платежи для всех студентов в группе
        if (model.StudentIds.Count != 0)
        {
            // Сначала создаем все записи GroupStudent
            foreach (var studentId in model.StudentIds)
            {
                var groupStudent = new GroupStudent
                {
                    Id = Guid.NewGuid(),
                    GroupId = group.Id,
                    StudentId = studentId,
                    DiscountType = DiscountType.Percentage,
                    DiscountValue = 0,
                    DiscountReason = null,
                    JoinedAt = DateTime.UtcNow
                };
                await _context.GroupStudents.AddAsync(groupStudent);
            }
            
            // Сохраняем GroupStudent записи в базу
            await _context.SaveChangesAsync();
            
            // Проверяем активность группы через Schedule
            var isGroupActive = await IsGroupActiveAsync(group.Id);
            
            // Создаем платежи только если группа активна
            if (isGroupActive)
            {
                await _paymentService.CreatePaymentsForStudentsAsync(model.StudentIds, group.Id);
            }
        }
        
        return group.Id;
    }

    public async Task<PagedResult<GroupsDto>> GetAllAsync(GetGroupsRequest request)
    {
        var query = _context.Groups
            .Include(x => x.Subject)
            .Include(x => x.Students)
            .Include(x => x.GroupStudents)
            .Where(x => x.OrganizationId == request.OrganizationId);

        // Фильтрация по предмету (необязательно)
        if (request.SubjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId == request.SubjectId.Value);
        }

        // Поиск по названию группы или имени студента (необязательно)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(x => 
                x.Name.ToLower().Contains(searchLower) || 
                x.Students.Any(s => s.FullName.ToLower().Contains(searchLower))
            );
        }

        var pagedGroups = await query
            .OrderBy(x => x.Name)
            .Select(group => new GroupsDto
            {
                Id = group.Id,
                Name = group.Name,
                Code = group.Code,
                Level = group.Level,
                Subject = new SubjectMinimalViewModel
                {
                    SubjectId = group.Subject.Id,
                    SubjectName = group.Subject.Name
                },
                Students = group.GroupStudents.Select(gs => new UserMinimalViewModel
                {
                    StudentId = gs.StudentId,
                    StudentName = gs.Student.FullName,
                    IsFrozen = gs.IsFrozen,
                    FrozenFrom = gs.FrozenFrom,
                    FrozenTo = gs.FrozenTo,
                    FreezeReason = gs.FreezeReason
                }).ToList()
            })
            .ToPagedResultAsync(request);

        return pagedGroups;
    }

    /// <summary>
    /// Проверяет активность группы через Schedule:
    /// Группа активна если существует расписание где EffectiveFrom <= текущая дата и (EffectiveTo == null или EffectiveTo >= текущая дата)
    /// </summary>
    private async Task<bool> IsGroupActiveAsync(Guid groupId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var hasActiveSchedule = await _context.Schedules
            .AnyAsync(s => s.GroupId == groupId 
                          && s.EffectiveFrom <= today 
                          && (s.EffectiveTo == null || s.EffectiveTo >= today));
        
        return hasActiveSchedule;
    }

    private static string GenerateCode()
    {
        var random = new Random();

        var letters = new string(Enumerable.Range(0, 2)
            .Select(_ => (char)random.Next('A', 'Z' + 1))
            .ToArray());

        var numbers = random.Next(0, 1000).ToString("D3");

        return $"{letters}-{numbers}";
    }
    
    public async Task FreezeStudentAsync(FreezeStudentRequest request)
    {
        var groupStudent = await _context.Set<GroupStudent>()
            .FirstOrDefaultAsync(gs => gs.GroupId == request.GroupId && gs.StudentId == request.StudentId);

        if (groupStudent == null)
        {
            throw new ConflictException("Студент не найден в этой группе.");
        }

        if (groupStudent.IsFrozen)
        {
            throw new ConflictException("Студент уже заморожен.");
        }

        groupStudent.IsFrozen = true;
        groupStudent.FrozenFrom = request.FrozenFrom;
        groupStudent.FrozenTo = request.FrozenTo;
        groupStudent.FreezeReason = request.FreezeReason;

        await _context.SaveChangesAsync();
    }
    
    public async Task UnfreezeStudentAsync(UnfreezeStudentRequest request)
    {
        var groupStudent = await _context.Set<GroupStudent>()
            .FirstOrDefaultAsync(gs => gs.GroupId == request.GroupId && gs.StudentId == request.StudentId);

        if (groupStudent == null)
        {
            throw new ConflictException("Студент не найден в этой группе.");
        }

        if (!groupStudent.IsFrozen)
        {
            throw new ConflictException("Студент не заморожен.");
        }

        // Подсчитываем количество пропущенных уроков за период заморозки
        var frozenFrom = groupStudent.FrozenFrom!.Value;
        var frozenTo = groupStudent.FrozenTo!.Value;
        
        var missedLessonsCount = await _context.Lessons
            .Where(l => l.GroupId == request.GroupId 
                       && l.Date >= frozenFrom 
                       && l.Date <= frozenTo)
            .CountAsync();

        // Находим активный платеж студента для этой группы
        var activePayment = await _context.Payments
            .Where(p => p.StudentId == request.StudentId 
                       && p.GroupId == request.GroupId 
                       && p.Status != PaymentStatus.Paid)
            .OrderByDescending(p => p.PeriodEnd)
            .FirstOrDefaultAsync();

        // Продлеваем платеж на количество пропущенных уроков (дней)
        if (activePayment != null)
        {
            activePayment.PeriodEnd = activePayment.PeriodEnd.AddDays(missedLessonsCount);
        }

        // Снимаем заморозку
        groupStudent.IsFrozen = false;
        groupStudent.FrozenFrom = null;
        groupStudent.FrozenTo = null;
        groupStudent.FreezeReason = null;

        await _context.SaveChangesAsync();
    }
    
    public async Task BulkAddStudentsAsync(BulkAddStudentsRequest request)
    {
        // Проверяем существование группы
        var group = await _context.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.Id == request.GroupId);

        if (group == null)
        {
            throw new ConflictException("Группа не найдена.");
        }

        if (request.StudentIds == null || request.StudentIds.Count == 0)
        {
            throw new ConflictException("Список студентов пуст.");
        }

        // Получаем ID студентов, которые уже есть в группе
        var existingStudentIds = group.Students.Select(s => s.Id).ToHashSet();
        
        // Фильтруем только новых студентов
        var newStudentIds = request.StudentIds
            .Distinct()
            .Where(id => !existingStudentIds.Contains(id))
            .ToList();

        if (newStudentIds.Count == 0)
        {
            throw new ConflictException("Все указанные студенты уже находятся в группе.");
        }

        // Проверяем, что все новые студенты существуют в базе
        var existingUsers = await _context.Users
            .Where(u => newStudentIds.Contains(u.Id) && u.Role == RoleEnum.Student)
            .Select(u => u.Id)
            .ToListAsync();

        if (existingUsers.Count != newStudentIds.Count)
        {
            throw new ConflictException("Некоторые из указанных ID не являются студентами или не существуют.");
        }

        // Создаем записи GroupStudent для новых студентов
        foreach (var studentId in newStudentIds)
        {
            var groupStudent = new GroupStudent
            {
                Id = Guid.NewGuid(),
                GroupId = request.GroupId,
                StudentId = studentId,
                DiscountType = DiscountType.Percentage,
                DiscountValue = 0,
                DiscountReason = null,
                JoinedAt = DateTime.UtcNow
            };
            await _context.GroupStudents.AddAsync(groupStudent);
        }
        
        // Сохраняем GroupStudent записи в базу
        await _context.SaveChangesAsync();
        
        // Проверяем активность группы через Schedule
        var isGroupActive = await IsGroupActiveAsync(request.GroupId);
        
        // Создаем платежи только если группа активна
        if (isGroupActive)
        {
            await _paymentService.CreatePaymentsForStudentsAsync(newStudentIds, request.GroupId);
        }
    }
}