using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.AssignmentServices.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Exception;
using Trackademy.Application.Shared.Extensions;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.AssignmentServices;

public class AssignmentService : BaseService<Assignment, AssignmentDto, AssignmentAddModel, AssignmentUpdateModel>, IAssignmentService
{
    public AssignmentService(TrackademyDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<PagedResult<AssignmentDto>> GetAllAsync(GetAssignmentsRequest request, Guid userId)
    {
        // Получаем роль пользователя
        var user = await _context.Set<User>()
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new ConflictException("User not found");

        var query = _context.Set<Assignment>()
            .Include(a => a.Group)
            .Where(a => a.Group.OrganizationId == request.OrganizationId);

        // Фильтрация по роли пользователя
        if (user.Role == RoleEnum.Teacher)
        {
            // Преподаватели видят только assignments своих групп
            var userGroupIds = user.Groups.Select(g => g.Id).ToList();
            query = query.Where(a => userGroupIds.Contains(a.GroupId));
        }
        // Студенты и администраторы видят все (по организации)

        if (request.GroupId.HasValue)
        {
            query = query.Where(a => a.GroupId == request.GroupId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.AssignedDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.DueDate <= request.ToDate.Value);
        }

        var assignments = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var assignmentDtos = _mapper.Map<List<AssignmentDto>>(assignments);

        return assignmentDtos.ToPagedResult(request);
    }

    public override async Task<AssignmentDto?> GetByIdAsync(Guid id)
    {
        var assignment = await _context.Set<Assignment>()
            .Include(a => a.Group)
            .FirstOrDefaultAsync(a => a.Id == id);
            
        return assignment != null ? _mapper.Map<AssignmentDto>(assignment) : null;
    }

    public override async Task<IEnumerable<AssignmentDto>> GetAllAsync()
    {
        var assignments = await _context.Set<Assignment>()
            .Include(a => a.Group)
            .ToListAsync();
            
        return _mapper.Map<IEnumerable<AssignmentDto>>(assignments);
    }

    public async Task<Guid> CreateAsync(AssignmentAddModel model, Guid userId)
    {
        // Проверяем, что пользователь - преподаватель и может создавать assignments для этой группы
        var user = await _context.Set<User>()
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new ConflictException("User not found");

        if (user.Role == RoleEnum.Teacher)
        {
            // Проверяем, что группа принадлежит этому преподавателю
            var userGroupIds = user.Groups.Select(g => g.Id).ToList();
            if (!userGroupIds.Contains(model.GroupId))
                throw new ConflictException("Teacher can only create assignments for their own groups");
        }

        return await CreateAsync(model);
    }

    public async Task<Guid> UpdateAsync(Guid id, AssignmentUpdateModel model, Guid userId)
    {
        // Получаем assignment для проверки
        var assignment = await _context.Set<Assignment>()
            .Include(a => a.Group)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null)
            throw new ConflictException("Assignment not found");

        var user = await _context.Set<User>()
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new ConflictException("User not found");

        if (user.Role == RoleEnum.Teacher)
        {
            // Проверяем, что assignment принадлежит группе преподавателя
            var userGroupIds = user.Groups.Select(g => g.Id).ToList();
            if (!userGroupIds.Contains(assignment.GroupId))
                throw new ConflictException("Teacher can only update assignments for their own groups");
        }

        return await UpdateAsync(id, model);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        // Получаем assignment для проверки
        var assignment = await _context.Set<Assignment>()
            .Include(a => a.Group)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null)
            return false;

        var user = await _context.Set<User>()
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new ConflictException("User not found");

        if (user.Role == RoleEnum.Teacher)
        {
            // Проверяем, что assignment принадлежит группе преподавателя
            var userGroupIds = user.Groups.Select(g => g.Id).ToList();
            if (!userGroupIds.Contains(assignment.GroupId))
                throw new ConflictException("Teacher can only delete assignments for their own groups");
        }

        return await DeleteAsync(id);
    }
}