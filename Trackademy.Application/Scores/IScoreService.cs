using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Scores
{
    public interface IScoreService
    {
        // CRUD операции
        Task<ScoreResponseModel> CreateScoreAsync(ScoreCreateModel model, Guid teacherId);
        Task<ScoreResponseModel> UpdateScoreAsync(Guid scoreId, ScoreUpdateModel model, Guid teacherId);
        Task DeleteScoreAsync(Guid scoreId, Guid teacherId);
        Task<ScoreResponseModel?> GetScoreByIdAsync(Guid scoreId);
        
        // Получение оценок
        Task<List<ScoreResponseModel>> GetScoresBySubmissionAsync(Guid submissionId);
        Task<List<ScoreResponseModel>> GetScoresByAssignmentAsync(Guid assignmentId);
        Task<List<ScoreResponseModel>> GetScoresByStudentAsync(Guid studentId);
        Task<ScoreResponseModel?> GetLatestScoreForSubmissionAsync(Guid submissionId);
        
        // История оценок
        Task<List<ScoreHistoryModel>> GetScoreHistoryAsync(Guid submissionId);
        
        // Статистика
        Task<ScoreStatisticsModel> GetAssignmentStatisticsAsync(Guid assignmentId);
        Task<Dictionary<Guid, double>> GetStudentAveragesAsync(Guid groupId, DateTime? fromDate = null, DateTime? toDate = null);
        
        // Bulk операции
        Task<List<ScoreResponseModel>> CreateBulkScoresAsync(BulkScoreModel model, Guid teacherId);
        Task RecalculateScoresAsync(Guid assignmentId);
        
        // Валидация
        Task<bool> ValidateScoreAsync(ScoreCreateModel model);
        Task<bool> CanTeacherGradeSubmissionAsync(Guid teacherId, Guid submissionId);
        

    }
}