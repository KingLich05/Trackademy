using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trackademy.Application.MaterialServices;
using Trackademy.Application.MaterialServices.Models;

namespace Trackademy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaterialController : ControllerBase
{
    private readonly IMaterialService _materialService;
    private readonly ILogger<MaterialController> _logger;

    public MaterialController(IMaterialService materialService, ILogger<MaterialController> logger)
    {
        _materialService = materialService;
        _logger = logger;
    }

    /// <summary>
    /// Загрузить учебный материал (для администраторов и преподавателей)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,Owner,Teacher")]
    public async Task<IActionResult> UploadMaterial([FromForm] MaterialAddModel model)
    {
        var userId = GetCurrentUserId();

        try
        {
            var material = await _materialService.UploadMaterialAsync(model, userId);
            return Ok(material);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Получить список материалов с фильтрацией и пагинацией
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMaterials([FromQuery] GetMaterialsRequest request)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        try
        {
            var materials = await _materialService.GetMaterialsAsync(request, userId, userRole);
            return Ok(materials);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials");
            return StatusCode(500, new { message = "Ошибка при получении материалов" });
        }
    }

    /// <summary>
    /// Получить материал по ID
    /// </summary>
    [HttpGet("{materialId}")]
    public async Task<IActionResult> GetById(Guid materialId)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        try
        {
            var material = await _materialService.GetByIdAsync(materialId, userId, userRole);
            return Ok(material);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Обновить название и описание материала (только автор или администратор)
    /// </summary>
    [HttpPut("{materialId}")]
    public async Task<IActionResult> UpdateMaterial(Guid materialId, [FromBody] MaterialUpdateModel model)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        try
        {
            var material = await _materialService.UpdateMaterialAsync(materialId, model, userId, userRole);
            return Ok(material);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удалить материал (только автор или администратор)
    /// </summary>
    [HttpDelete("{materialId}")]
    public async Task<IActionResult> DeleteMaterial(Guid materialId)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        try
        {
            await _materialService.DeleteMaterialAsync(materialId, userId, userRole);
            return Ok(new { message = "Материал удален" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Скачать файл материала
    /// </summary>
    [HttpGet("{materialId}/download")]
    public async Task<IActionResult> DownloadMaterial(Guid materialId)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        try
        {
            var fileResult = await _materialService.DownloadMaterialAsync(materialId, userId, userRole);
            return File(fileResult.Content, fileResult.ContentType, fileResult.FileName);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? throw new UnauthorizedAccessException();
    }
}
