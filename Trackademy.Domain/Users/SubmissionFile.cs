using Trackademy.Domain.Common;

namespace Trackademy.Domain.Users;

public class SubmissionFile : Entity
{
    public Guid SubmissionId { get; set; }
    
    /// <summary>
    /// Оригинальное имя файла
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Имя файла в файловой системе (уникальное)
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME тип файла
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Путь к файлу в хранилище
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата загрузки файла
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Является ли файл изображением
    /// </summary>
    public bool IsImage { get; set; }

    // Navigation properties
    public Submission Submission { get; set; } = null!;
}