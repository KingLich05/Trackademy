using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
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
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new ConflictException("User not found");

        var query = _context.Set<Assignment>()
            .Include(a => a.Group)
            .Where(a => a.Group.OrganizationId == request.OrganizationId);

        // Фильтрация по роли пользователя
        if (user.Role == RoleEnum.Teacher)
        {
            // Преподаватели видят только assignments своих групп (через Schedule)
            var teacherGroupIds = await _context.Set<Domain.Users.Schedule>()
                .Where(s => s.TeacherId == userId)
                .Select(s => s.GroupId)
                .Distinct()
                .ToListAsync();
                
            query = query.Where(a => teacherGroupIds.Contains(a.GroupId));
        }
        // Студенты и администраторы видят все (по организации)

        if (request.GroupId.HasValue)
        {
            query = query.Where(a => a.GroupId == request.GroupId.Value);
        }

        // Фильтрация по периоду: возвращаем задания, диапазон которых [AssignedDate, DueDate]
        // пересекается с указанным диапазоном [FromDate, ToDate].
        // Диапазоны пересекаются, если: AssignedDate <= ToDate AND DueDate >= FromDate
        if (request.FromDate.HasValue || request.ToDate.HasValue)
        {
            var from = request.FromDate ?? DateOnly.MinValue;
            var to = request.ToDate ?? DateOnly.MaxValue;

            query = query.Where(a =>
                DateOnly.FromDateTime(a.AssignedDate) <= to &&
                DateOnly.FromDateTime(a.DueDate) >= from
            );
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
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new ConflictException("Пользователь не найден.");

        if (user.Role == RoleEnum.Teacher)
        {
            var teacherGroupIds = await _context.Set<Domain.Users.Schedule>()
                .Where(s => s.TeacherId == userId)
                .Select(s => s.GroupId)
                .Distinct()
                .ToListAsync();

            if (!teacherGroupIds.Contains(model.GroupId))
                throw new ConflictException("Учитель может создавать домашнее задание только для своих групп.");
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
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new ConflictException("User not found");

        if (user.Role == RoleEnum.Teacher)
        {
            // Получаем группы преподавателя через расписание
            var teacherGroupIds = await _context.Set<Domain.Users.Schedule>()
                .Where(s => s.TeacherId == userId)
                .Select(s => s.GroupId)
                .Distinct()
                .ToListAsync();

            if (!teacherGroupIds.Contains(assignment.GroupId))
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
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new ConflictException("User not found");

        if (user.Role == RoleEnum.Teacher)
        {
            // Получаем группы преподавателя через расписание
            var teacherGroupIds = await _context.Set<Domain.Users.Schedule>()
                .Where(s => s.TeacherId == userId)
                .Select(s => s.GroupId)
                .Distinct()
                .ToListAsync();

            if (!teacherGroupIds.Contains(assignment.GroupId))
                throw new ConflictException("Teacher can only delete assignments for their own groups");
        }

        return await DeleteAsync(id);
    }

    public async Task<AssignmentDetailedDto?> GetByIdWithSubmissionsAsync(Guid assignmentId, Guid userId, string userRole)
    {
        // Загружаем assignment с группой и студентами
        var assignment = await _context.Set<Assignment>()
            .Include(a => a.Group)
                .ThenInclude(g => g.Students)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment == null)
            return null;

        // Загружаем все submissions для этого assignment
        var submissions = await _context.Set<Submission>()
            .Where(s => s.AssignmentId == assignmentId)
            .Include(s => s.Scores)
            .ToListAsync();

        // Маппим базовую информацию об assignment
        var result = _mapper.Map<AssignmentDetailedDto>(assignment);

        // Формируем список студентов с их submissions
        var studentSubmissions = assignment.Group.Students
            .Select(student =>
            {
                var submission = submissions.FirstOrDefault(s => s.StudentId == student.Id);
                
                return new StudentSubmissionDto
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    StudentLogin = student.Login,
                    Submission = submission != null
                        ? new StudentSubmissionInfo
                        {
                            Id = submission.Id,
                            Status = (int)submission.Status,
                            StatusName = submission.Status.ToString(),
                            Score = submission.Scores.FirstOrDefault()?.NumericValue,
                            SubmittedAt = submission.SubmittedAt,
                            GradedAt = submission.GradedAt
                        }
                        : null
                };
            })
            .ToList();

        // Фильтрация по роли: студенты видят только свой submission
        if (userRole == RoleEnum.Student.ToString())
        {
            studentSubmissions = studentSubmissions
                .Where(s => s.StudentId == userId)
                .ToList();
        }

        result.StudentSubmissions = studentSubmissions;

        return result;
    }
}