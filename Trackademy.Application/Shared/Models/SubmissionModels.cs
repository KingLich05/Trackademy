using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Shared.Models
{
    public class SubmissionCreateUpdateModel
    {
        [StringLength(10000, ErrorMessage = "Текст ответа не может превышать 10000 символов")]
        public string? TextContent { get; set; }

        [Display(Name = "Файлы")]
        public List<IFormFile>? Files { get; set; }
    }

    public class GradeSubmissionModel
    {
        [Required]
        [Range(0, 100, ErrorMessage = "Оценка должна быть от 0 до 100")]
        public int Score { get; set; }

        [StringLength(2000, ErrorMessage = "Комментарий не может превышать 2000 символов")]
        public string? TeacherComment { get; set; }
    }

    public class ReturnSubmissionModel
    {
        [Required]
        [StringLength(2000, ErrorMessage = "Комментарий обязателен и не может превышать 2000 символов")]
        public string TeacherComment { get; set; } = string.Empty;
    }

    public class SubmissionResponseModel
    {
        public Guid Id { get; set; }
        public Guid AssignmentId { get; set; }
        public Guid StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? TextContent { get; set; }
        public SubmissionStatus Status { get; set; }
        public int? Score { get; set; }
        public string? TeacherComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? GradedAt { get; set; }
        public List<SubmissionFileResponseModel> Files { get; set; } = new();
    }

    public class SubmissionFileResponseModel
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsImage { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
    }

    public class FileDownloadResult
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}