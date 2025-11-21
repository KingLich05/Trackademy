using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.GroupServices.Models;
using Trackademy.Application.PaymentServices;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Exception;
using Trackademy.Application.Shared.Extensions;
using Trackademy.Application.Shared.Models;
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
                
                // Теперь создаем платежи для новых студентов
                foreach (var studentId in idsToAdd)
                {
                    await _paymentService.CreatePaymentForStudentAsync(studentId, entity.Id);
                }
            }
        }

        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<List<GroupsDto>> GetAllAsync(Guid organizationId)
    {
        var group = await _context.Groups
            .Include(x => x.Students)
            .Include(x => x.Subject)
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var groupsTdo = _mapper.Map<List<GroupsDto>>(group);

        return groupsTdo;
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
            
            // Теперь создаем платежи для всех студентов
            foreach (var studentId in model.StudentIds)
            {
                await _paymentService.CreatePaymentForStudentAsync(studentId, group.Id);
            }
        }
        
        return group.Id;
    }

    public async Task<PagedResult<GroupsDto>> GetAllAsync(GetGroupsRequest request)
    {
        var query = _context.Groups
            .Include(x => x.Subject)
            .Include(x => x.Students)
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
            .Select(group => _mapper.Map<GroupsDto>(group))
            .ToPagedResultAsync(request);

        return pagedGroups;
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
}