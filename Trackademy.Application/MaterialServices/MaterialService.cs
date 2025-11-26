using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Trackademy.Application.MaterialServices.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.MaterialServices;

public class MaterialService : IMaterialService
{
    private readonly TrackademyDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _uploadPath;
    private readonly long _maxFileSize = 150 * 1024 * 1024; // 150MB
    private readonly string[] _allowedExtensions = 
    {
        ".pdf", ".doc", ".docx", ".txt", ".rtf",
        ".ppt", ".pptx", ".xls", ".xlsx", ".csv",
        ".jpg", ".jpeg", ".png", ".gif",
        ".zip", ".rar", ".7z",
        ".epub", ".djvu"
    };

    public MaterialService(TrackademyDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _uploadPath = _configuration["FileStorage:Materials"] ?? "uploads/materials";

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<MaterialDto> UploadMaterialAsync(MaterialAddModel model, Guid uploadedById)
    {
        // Валидация файла
        ValidateFile(model.File);

        // Проверка существования группы
        var group = await _context.Groups.FindAsync(model.GroupId);
        if (group == null)
            throw new InvalidOperationException("Группа не найдена");

        // Проверка прав доступа (только admin или teacher могут загружать)
        var user = await _context.Users.FindAsync(uploadedById);
        if (user == null || (user.Role != RoleEnum.Administrator && user.Role != RoleEnum.Owner && user.Role != RoleEnum.Teacher))
            throw new UnauthorizedAccessException("Только администраторы и преподаватели могут загружать материалы");

        var now = DateTime.UtcNow;
        var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var materialId = Guid.NewGuid();
        var relativePath = Path.Combine(materialId.ToString(), storedFileName);
        var fullPath = Path.Combine(_uploadPath, relativePath);

        // Создание директории для материала
        var materialDirectory = Path.Combine(_uploadPath, materialId.ToString());
        if (!Directory.Exists(materialDirectory))
        {
            Directory.CreateDirectory(materialDirectory);
        }

        // Сохранение файла
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await model.File.CopyToAsync(stream);
        }

        // Создание записи в БД
        var material = new Material
        {
            Id = materialId,
            Title = model.Title,
            Description = model.Description,
            OriginalFileName = model.File.FileName,
            StoredFileName = storedFileName,
            ContentType = model.File.ContentType,
            FileSize = model.File.Length,
            FilePath = relativePath,
            GroupId = model.GroupId,
            UploadedById = uploadedById,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Materials.AddAsync(material);
        await _context.SaveChangesAsync();

        await _context.Entry(material)
            .Reference(m => m.Group)
            .LoadAsync();
        await _context.Entry(material)
            .Reference(m => m.UploadedBy)
            .LoadAsync();

        return MapToDto(material);
    }

    public async Task<PagedResult<MaterialDto>> GetMaterialsAsync(GetMaterialsRequest request, Guid userId, string userRole)
    {
        var query = _context.Materials
            .Include(m => m.Group)
            .Include(m => m.UploadedBy)
            .AsQueryable();

        // Фильтрация по организации
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(m => m.Group.OrganizationId == request.OrganizationId.Value);
        }

        // Фильтрация по группе
        if (request.GroupId.HasValue)
        {
            query = query.Where(m => m.GroupId == request.GroupId.Value);
        }

        // Поиск по названию
        if (!string.IsNullOrWhiteSpace(request.SearchTitle))
        {
            query = query.Where(m => m.Title.Contains(request.SearchTitle));
        }

        // Права доступа
        if (userRole == RoleEnum.Student.ToString())
        {
            // Студенты видят только материалы своих групп
            var studentGroupIds = await _context.GroupStudents
                .Where(gs => gs.StudentId == userId)
                .Select(gs => gs.GroupId)
                .ToListAsync();

            query = query.Where(m => studentGroupIds.Contains(m.GroupId));
        }
        else if (userRole == RoleEnum.Teacher.ToString())
        {
            // Преподаватели видят материалы своих групп + свои загрузки
            var teacherGroupIds = await _context.Schedules
                .Where(s => s.TeacherId == userId)
                .Select(s => s.GroupId)
                .Distinct()
                .ToListAsync();

            query = query.Where(m => teacherGroupIds.Contains(m.GroupId) || m.UploadedById == userId);
        }
        // Admin и Owner видят все

        // Сортировка
        query = request.SortByDateDescending
            ? query.OrderByDescending(m => m.CreatedAt)
            : query.OrderBy(m => m.CreatedAt);

        var totalCount = await query.CountAsync();

        var materials = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<MaterialDto>
        {
            Items = materials.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<MaterialDto> GetByIdAsync(Guid materialId, Guid userId, string userRole)
    {
        var material = await _context.Materials
            .Include(m => m.Group)
            .Include(m => m.UploadedBy)
            .FirstOrDefaultAsync(m => m.Id == materialId);

        if (material == null)
            throw new InvalidOperationException("Материал не найден");

        // Проверка прав доступа
        await CheckAccessRightsAsync(material, userId, userRole);

        return MapToDto(material);
    }

    public async Task<MaterialDto> UpdateMaterialAsync(Guid materialId, MaterialUpdateModel model, Guid userId, string userRole)
    {
        var material = await _context.Materials
            .Include(m => m.Group)
            .Include(m => m.UploadedBy)
            .FirstOrDefaultAsync(m => m.Id == materialId);

        if (material == null)
            throw new InvalidOperationException("Материал не найден");

        // Только автор или admin могут редактировать
        if (material.UploadedById != userId && userRole != RoleEnum.Administrator.ToString() && userRole != RoleEnum.Owner.ToString())
            throw new UnauthorizedAccessException("Нет прав для редактирования этого материала");

        material.Title = model.Title;
        material.Description = model.Description;
        material.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(material);
    }

    public async Task DeleteMaterialAsync(Guid materialId, Guid userId, string userRole)
    {
        var material = await _context.Materials.FindAsync(materialId);

        if (material == null)
            throw new InvalidOperationException("Материал не найден");

        // Только автор или admin могут удалять
        if (material.UploadedById != userId && userRole != RoleEnum.Administrator.ToString() && userRole != RoleEnum.Owner.ToString())
            throw new UnauthorizedAccessException("Нет прав для удаления этого материала");

        // Удаление файла с диска
        var fullPath = Path.Combine(_uploadPath, material.FilePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        // Удаление директории материала, если она пуста
        var materialDirectory = Path.Combine(_uploadPath, material.Id.ToString());
        if (Directory.Exists(materialDirectory) && !Directory.EnumerateFileSystemEntries(materialDirectory).Any())
        {
            Directory.Delete(materialDirectory);
        }

        _context.Materials.Remove(material);
        await _context.SaveChangesAsync();
    }

    public async Task<FileDownloadResult> DownloadMaterialAsync(Guid materialId, Guid userId, string userRole)
    {
        var material = await _context.Materials
            .Include(m => m.Group)
            .FirstOrDefaultAsync(m => m.Id == materialId);

        if (material == null)
            throw new InvalidOperationException("Материал не найден");

        // Проверка прав доступа
        await CheckAccessRightsAsync(material, userId, userRole);

        var fullPath = Path.Combine(_uploadPath, material.FilePath);

        if (!File.Exists(fullPath))
            throw new InvalidOperationException("Файл не найден на сервере");

        var fileBytes = await File.ReadAllBytesAsync(fullPath);

        return new FileDownloadResult
        {
            Content = fileBytes,
            FileName = material.OriginalFileName,
            ContentType = material.ContentType
        };
    }

    private void ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
            throw new InvalidOperationException("Файл пустой");

        if (file.Length > _maxFileSize)
            throw new InvalidOperationException($"Размер файла превышает {_maxFileSize / (1024 * 1024)} MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"Недопустимый тип файла: {extension}");
    }

    private async Task CheckAccessRightsAsync(Material material, Guid userId, string userRole)
    {
        if (userRole == RoleEnum.Administrator.ToString() || userRole == RoleEnum.Owner.ToString())
            return; // Admin и Owner имеют полный доступ

        if (userRole == RoleEnum.Teacher.ToString())
        {
            // Преподаватель может видеть материалы своих групп или свои загрузки
            if (material.UploadedById == userId)
                return;

            var teacherGroupIds = await _context.Schedules
                .Where(s => s.TeacherId == userId)
                .Select(s => s.GroupId)
                .ToListAsync();

            if (teacherGroupIds.Contains(material.GroupId))
                return;
        }
        else if (userRole == RoleEnum.Student.ToString())
        {
            // Студент может видеть только материалы своих групп
            var isInGroup = await _context.GroupStudents
                .AnyAsync(gs => gs.StudentId == userId && gs.GroupId == material.GroupId);

            if (isInGroup)
                return;
        }

        throw new UnauthorizedAccessException("Нет доступа к этому материалу");
    }

    private MaterialDto MapToDto(Material material)
    {
        return new MaterialDto
        {
            Id = material.Id,
            Title = material.Title,
            Description = material.Description,
            OriginalFileName = material.OriginalFileName,
            ContentType = material.ContentType,
            FileSize = material.FileSize,
            GroupId = material.GroupId,
            GroupName = material.Group?.Name ?? string.Empty,
            UploadedById = material.UploadedById,
            UploadedByName = material.UploadedBy?.FullName ?? string.Empty,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt
        };
    }
}
