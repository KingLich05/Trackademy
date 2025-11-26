using Trackademy.Domain.Common;

namespace Trackademy.Domain.Users;

/// <summary>
/// Учебный материал для группы
/// </summary>
public class Material : Entity
{
    /// <summary>
    /// Название материала
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Описание материала (опционально)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Оригинальное имя файла
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Уникальное имя файла в хранилище
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Относительный путь к файлу
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME тип файла
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// ID группы, к которой относится материал
    /// </summary>
    public Guid GroupId { get; set; }
    
    /// <summary>
    /// ID пользователя, который загрузил материал
    /// </summary>
    public Guid UploadedById { get; set; }
    
    /// <summary>
    /// Дата загрузки
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Groups Group { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}
