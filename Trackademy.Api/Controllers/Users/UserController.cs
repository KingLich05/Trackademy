using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Application.authenticator.Models;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IUserServices service) : ControllerBase
{
    
    [HttpGet("GetUserById/{id:guid}")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetUserById(
        Guid id)
    {
        var user = await service.GetById(id);
        
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("create")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await service.CreateUser(request);

        return CreatedAtAction(nameof(GetUserById), new { id = result.User!.Id }, result.User);
    }

    [HttpPost("get-users")]
    [RoleAuthorization(RoleEnum.Teacher)]
    public async Task<IActionResult> GetUsers(
        [FromBody] GetUserRequest getUserRequest)
    {
        var users = await service.GetUsers(getUserRequest);
        return Ok(users);
    }

    [HttpPut("update-user/{id:guid}")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UserUpdateModel getUserRequest)
    {
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (currentUserIdClaim == null)
            return Unauthorized("Не удалось определить текущего пользователя");

        if (!Guid.TryParse(currentUserIdClaim.Value, out var currentUserId))
            return Unauthorized("Неверный ID пользователя");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole == RoleEnum.Administrator.ToString() || userRole == RoleEnum.Owner.ToString())
        {
            var result = await service.UpdateUser(id, getUserRequest);
            return Ok(result);
        }

        if (userRole == RoleEnum.Teacher.ToString() || userRole == RoleEnum.Student.ToString())
        {
            if (currentUserId != id)
            {
                return Forbid();
            }

            var result = await service.UpdateUser(id, getUserRequest);
            return Ok(result);
        }

        return Forbid();
    }

    [HttpPut("update-password")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (currentUserIdClaim == null)
            return Unauthorized("Не удалось определить текущего пользователя");

        if (!Guid.TryParse(currentUserIdClaim.Value, out var currentUserId))
            return Unauthorized("Неверный ID пользователя");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Администраторы и владельцы могут менять пароль любому пользователю
        if (userRole == RoleEnum.Administrator.ToString() || userRole == RoleEnum.Owner.ToString())
        {
            var result = await service.UpdatePassword(request);
            if (!result)
                return BadRequest("Не удалось обновить пароль. Проверьте правильность текущего пароля и существование пользователя.");
            
            return Ok(new { message = "Пароль успешно обновлен" });
        }

        if (userRole == RoleEnum.Teacher.ToString() || userRole == RoleEnum.Student.ToString())
        {
            if (currentUserId != request.StudentId)
            {
                return Forbid();
            }

            var result = await service.UpdatePassword(request);
            if (!result)
                return BadRequest("Не удалось обновить пароль. Проверьте правильность текущего пароля.");
            
            return Ok(new { message = "Пароль успешно обновлен" });
        }

        return Forbid();
    }

    [HttpPost("import-excel")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> ImportUsersFromExcel(
        [FromForm] IFormFile file,
        [FromForm] Guid organizationId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не загружен");

        // Проверка типа файла
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
            return BadRequest("Допустимы только файлы Excel (.xlsx, .xls)");

        try
        {
            // Парсим Excel файл
            var excelService = HttpContext.RequestServices.GetRequiredService<IExcelImportService>();
            var rows = await excelService.ParseExcelFile(file);

            if (!rows.Any())
                return BadRequest("Файл не содержит данных для импорта");

            // Импортируем пользователей
            var result = await service.ImportUsersFromExcel(rows, organizationId);

            // Возвращаем результат с детальной информацией
            return Ok(new
            {
                result.TotalRows,
                result.SuccessCount,
                result.ErrorCount,
                result.CreatedUsers,
                result.Errors
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Ошибка при обработке файла: {ex.Message}");
        }
    }

    [HttpDelete("{id:guid}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> DeleteUser(
        Guid id)
    {
        var result = await service.DeleteUser(id);

        if (!result) return NotFound();

        return Ok(result);
    }
}