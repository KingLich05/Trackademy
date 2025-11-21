using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Application.AssignmentServices;
using Trackademy.Application.AssignmentServices.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers.Assignment;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssignmentController(IAssignmentService service) : ControllerBase
{
    /// <summary>
    /// Получение assignment по ID с информацией о студентах и их submissions
    /// Студенты видят только свой submission, учителя и администраторы - всех студентов
    /// </summary>
    [HttpGet("{id}")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? RoleEnum.Student.ToString();

        var assignment = await service.GetByIdWithSubmissionsAsync(id, userId, userRole);
        if (assignment == null)
            return NotFound();

        return Ok(assignment);
    }

    /// <summary>
    /// Получить ID текущего пользователя из JWT токена
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var id))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return id;
    }

    /// <summary>
    /// Получение списка assignments с фильтрацией - доступно всем авторизованным пользователям
    /// </summary>
    [HttpPost("get-assignments")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetAssignments([FromBody] GetAssignmentsRequest request)
    {
        var userId = GetCurrentUserId();
        var assignments = await service.GetAllAsync(request, userId);
        return Ok(assignments);
    }

    /// <summary>
    /// Создание нового assignment - только преподаватели и выше
    /// </summary>
    [HttpPost("create")]
    [RoleAuthorization(RoleEnum.Teacher)]
    public async Task<IActionResult> Create([FromBody] AssignmentAddModel model)
    {
        var userId = GetCurrentUserId();
        var id = await service.CreateAsync(model, userId);
        return Ok(id);
    }

    /// <summary>
    /// Обновление assignment - только преподаватели и выше
    /// </summary>
    [HttpPut("{id}")]
    [RoleAuthorization(RoleEnum.Teacher)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AssignmentUpdateModel model)
    {
        var userId = GetCurrentUserId();
        var updated = await service.UpdateAsync(id, model, userId);
        if (updated == Guid.Empty)
            return NotFound();

        return Ok(updated);
    }

    /// <summary>
    /// Удаление assignment - только преподаватели и выше
    /// </summary>
    [HttpDelete("{id}")]
    [RoleAuthorization(RoleEnum.Teacher)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        var isDeleted = await service.DeleteAsync(id, userId);
        if (!isDeleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Получение всех заданий студента с группировкой по статусам
    /// Возвращает:
    /// - pending: задания в работе или не начатые
    /// - submitted: отправленные на проверку
    /// - graded: проверенные и оцененные
    /// - overdue: просроченные
    /// </summary>
    [HttpPost("my-assignments")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetMyAssignments([FromBody] MyAssignmentsRequest request)
    {
        var studentId = GetCurrentUserId();
        var result = await service.GetMyAssignmentsAsync(request.OrganizationId, studentId);
        return Ok(result);
    }
}