using System.ComponentModel.DataAnnotations;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Shared.Models
{
    public class ScoreCreateModel
    {
        [Required]
        public Guid SubmissionId { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Числовая оценка должна быть больше 0")]
        public int NumericValue { get; set; }
        
        [Range(1, 1000, ErrorMessage = "Максимальные баллы должны быть от 1 до 1000")]
        public int MaxPoints { get; set; } = 100;
        
        [StringLength(2000, ErrorMessage = "Комментарий не может превышать 2000 символов")]
        public string? Feedback { get; set; }
        
        public ScoreStatus Status { get; set; } = ScoreStatus.Final;
    }
    
    public class ScoreUpdateModel
    {
        [Range(0, int.MaxValue, ErrorMessage = "Числовая оценка должна быть больше 0")]
        public int? NumericValue { get; set; }
        
        [Range(1, 1000, ErrorMessage = "Максимальные баллы должны быть от 1 до 1000")]
        public int? MaxPoints { get; set; }
        
        [StringLength(2000, ErrorMessage = "Комментарий не может превышать 2000 символов")]
        public string? Feedback { get; set; }
        
        public ScoreStatus? Status { get; set; }
    }
    
    public class ScoreResponseModel
    {
        public Guid Id { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public int? NumericValue { get; set; }
        public int MaxPoints { get; set; }
        public string? Feedback { get; set; }
        public ScoreStatus Status { get; set; }
        public DateTime AwardedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Version { get; set; }
        public Guid? PreviousVersionId { get; set; }
        
        // Calculated fields
        public double PercentageScore { get; set; }
        public string DisplayValue { get; set; } = string.Empty;
    }
    
    public class ScoreHistoryModel
    {
        public Guid Id { get; set; }
        public string DisplayValue { get; set; } = string.Empty;
        public double PercentageScore { get; set; }
        public string? Feedback { get; set; }
        public ScoreStatus Status { get; set; }
        public DateTime AwardedAt { get; set; }
        public int Version { get; set; }
        public string? TeacherName { get; set; }
    }
    
    public class ScoreStatisticsModel
    {
        public Guid AssignmentId { get; set; }
        public int TotalSubmissions { get; set; }
        public int GradedSubmissions { get; set; }
        public double AverageScore { get; set; }
        public double MedianScore { get; set; }
        public double MinScore { get; set; }
        public double MaxScore { get; set; }
        public Dictionary<string, int> GradeDistribution { get; set; } = new();
    }
    
    public class BulkScoreModel
    {
        public List<ScoreCreateModel> Scores { get; set; } = new();
    }
}