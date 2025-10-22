using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Scores
{
    public class ScoreService : IScoreService
    {
        private readonly TrackademyDbContext _context;

        public ScoreService(TrackademyDbContext context)
        {
            _context = context;
        }

        public async Task<ScoreResponseModel> CreateScoreAsync(ScoreCreateModel model, Guid teacherId)
        {
            // Валидация
            if (!await ValidateScoreAsync(model))
                throw new InvalidOperationException("Некорректные данные оценки");

            if (!await CanTeacherGradeSubmissionAsync(teacherId, model.SubmissionId))
                throw new UnauthorizedAccessException("Нет прав для выставления оценки этой работе");

            // Проверяем, есть ли уже финальная оценка
            var existingScore = await _context.Scores
                .Where(s => s.SubmissionId == model.SubmissionId && s.Status == ScoreStatus.Final)
                .FirstOrDefaultAsync();

            var now = DateTime.UtcNow;
            var newScore = new Score
            {
                Id = Guid.NewGuid(),
                SubmissionId = model.SubmissionId,
                TeacherId = teacherId,
                NumericValue = model.NumericValue,
                MaxPoints = model.MaxPoints,
                Feedback = model.Feedback,
                Status = model.Status,
                AwardedAt = now,
                UpdatedAt = now,
                Version = 1
            };

            // Если заменяем существующую оценку, создаем версию
            if (existingScore != null)
            {
                existingScore.Status = ScoreStatus.Cancelled;
                newScore.PreviousVersionId = existingScore.Id;
                newScore.Version = existingScore.Version + 1;
            }

            _context.Scores.Add(newScore);
            await _context.SaveChangesAsync();

            // Обновляем submission
            await UpdateSubmissionGradeStatus(model.SubmissionId);

            return await MapToResponseModel(newScore);
        }

        public async Task<ScoreResponseModel> UpdateScoreAsync(Guid scoreId, ScoreUpdateModel model, Guid teacherId)
        {
            var score = await _context.Scores
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(s => s.Id == scoreId);

            if (score == null)
                throw new InvalidOperationException("Оценка не найдена");

            if (score.TeacherId != teacherId)
                throw new UnauthorizedAccessException("Можно изменять только свои оценки");

            if (score.Status == ScoreStatus.Cancelled)
                throw new InvalidOperationException("Нельзя изменить отмененную оценку");

            // Создаем новую версию вместо изменения существующей
            var newScore = new Score
            {
                Id = Guid.NewGuid(),
                SubmissionId = score.SubmissionId,
                TeacherId = teacherId,
                NumericValue = model.NumericValue ?? score.NumericValue,
                MaxPoints = model.MaxPoints ?? score.MaxPoints,
                Feedback = model.Feedback ?? score.Feedback,
                Status = model.Status ?? score.Status,
                AwardedAt = score.AwardedAt,
                UpdatedAt = DateTime.UtcNow,
                Version = score.Version + 1,
                PreviousVersionId = score.Id
            };

            // Отменяем предыдущую версию
            score.Status = ScoreStatus.Cancelled;

            _context.Scores.Add(newScore);
            await _context.SaveChangesAsync();

            return await MapToResponseModel(newScore);
        }

        public async Task DeleteScoreAsync(Guid scoreId, Guid teacherId)
        {
            var score = await _context.Scores
                .FirstOrDefaultAsync(s => s.Id == scoreId);

            if (score == null)
                throw new InvalidOperationException("Оценка не найдена");

            if (score.TeacherId != teacherId)
                throw new UnauthorizedAccessException("Можно удалять только свои оценки");

            score.Status = ScoreStatus.Cancelled;
            await _context.SaveChangesAsync();

            // Обновляем статус submission
            await UpdateSubmissionGradeStatus(score.SubmissionId);
        }

        public async Task<ScoreResponseModel?> GetScoreByIdAsync(Guid scoreId)
        {
            var score = await _context.Scores
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(s => s.Id == scoreId);

            return score == null ? null : await MapToResponseModel(score);
        }

        public async Task<List<ScoreResponseModel>> GetScoresBySubmissionAsync(Guid submissionId)
        {
            var scores = await _context.Scores
                .Include(s => s.Teacher)
                .Where(s => s.SubmissionId == submissionId && s.Status != ScoreStatus.Cancelled)
                .OrderByDescending(s => s.Version)
                .ToListAsync();

            var result = new List<ScoreResponseModel>();
            foreach (var score in scores)
            {
                result.Add(await MapToResponseModel(score));
            }
            return result;
        }

        public async Task<List<ScoreResponseModel>> GetScoresByAssignmentAsync(Guid assignmentId)
        {
            var scores = await _context.Scores
                .Include(s => s.Teacher)
                .Include(s => s.Submission)
                .Where(s => s.Submission.AssignmentId == assignmentId && s.Status == ScoreStatus.Final)
                .ToListAsync();

            var result = new List<ScoreResponseModel>();
            foreach (var score in scores)
            {
                result.Add(await MapToResponseModel(score));
            }
            return result;
        }

        public async Task<List<ScoreResponseModel>> GetScoresByStudentAsync(Guid studentId)
        {
            var scores = await _context.Scores
                .Include(s => s.Teacher)
                .Include(s => s.Submission)
                .Where(s => s.Submission.StudentId == studentId && s.Status == ScoreStatus.Final)
                .OrderByDescending(s => s.AwardedAt)
                .ToListAsync();

            var result = new List<ScoreResponseModel>();
            foreach (var score in scores)
            {
                result.Add(await MapToResponseModel(score));
            }
            return result;
        }

        public async Task<ScoreResponseModel?> GetLatestScoreForSubmissionAsync(Guid submissionId)
        {
            var score = await _context.Scores
                .Include(s => s.Teacher)
                .Where(s => s.SubmissionId == submissionId && s.Status == ScoreStatus.Final)
                .OrderByDescending(s => s.Version)
                .FirstOrDefaultAsync();

            return score == null ? null : await MapToResponseModel(score);
        }

        public async Task<List<ScoreHistoryModel>> GetScoreHistoryAsync(Guid submissionId)
        {
            var scores = await _context.Scores
                .Include(s => s.Teacher)
                .Where(s => s.SubmissionId == submissionId)
                .OrderByDescending(s => s.Version)
                .ToListAsync();

            return scores.Select(s => new ScoreHistoryModel
            {
                Id = s.Id,
                DisplayValue = s.GetDisplayValue(),
                PercentageScore = s.GetPercentageScore(),
                Feedback = s.Feedback,
                Status = s.Status,
                AwardedAt = s.AwardedAt,
                Version = s.Version,
                TeacherName = s.Teacher?.FullName
            }).ToList();
        }

        public async Task<ScoreStatisticsModel> GetAssignmentStatisticsAsync(Guid assignmentId)
        {
            var scores = await _context.Scores
                .Include(s => s.Submission)
                .Where(s => s.Submission.AssignmentId == assignmentId && s.Status == ScoreStatus.Final)
                .ToListAsync();

            var percentageScores = scores.Select(s => s.GetPercentageScore()).ToList();
            var totalSubmissions = await _context.Submissions
                .CountAsync(s => s.AssignmentId == assignmentId);

            return new ScoreStatisticsModel
            {
                AssignmentId = assignmentId,
                TotalSubmissions = totalSubmissions,
                GradedSubmissions = scores.Count,
                AverageScore = percentageScores.Any() ? percentageScores.Average() : 0,
                MedianScore = percentageScores.Any() ? GetMedian(percentageScores) : 0,
                MinScore = percentageScores.Any() ? percentageScores.Min() : 0,
                MaxScore = percentageScores.Any() ? percentageScores.Max() : 0,
                GradeDistribution = GetGradeDistribution(scores)
            };
        }

        public async Task<Dictionary<Guid, double>> GetStudentAveragesAsync(Guid groupId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Scores
                .Include(s => s.Submission)
                .ThenInclude(sub => sub.Assignment)
                .Where(s => s.Status == ScoreStatus.Final);

            // Фильтр по датам если указаны
            if (fromDate.HasValue)
                query = query.Where(s => s.AwardedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(s => s.AwardedAt <= toDate.Value);

            var scores = await query.ToListAsync();

            return scores
                .GroupBy(s => s.Submission.StudentId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(s => s.GetPercentageScore()).Average()
                );
        }

        public async Task<List<ScoreResponseModel>> CreateBulkScoresAsync(BulkScoreModel model, Guid teacherId)
        {
            var results = new List<ScoreResponseModel>();

            foreach (var scoreModel in model.Scores)
            {
                try
                {
                    var result = await CreateScoreAsync(scoreModel, teacherId);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    // Логируем ошибку и продолжаем
                    Console.WriteLine($"Error creating score for submission {scoreModel.SubmissionId}: {ex.Message}");
                }
            }

            return results;
        }

        public async Task RecalculateScoresAsync(Guid assignmentId)
        {
            // Здесь можно добавить логику пересчета оценок
            // например, при изменении максимального балла за задание
            await Task.CompletedTask;
        }

        public async Task<bool> ValidateScoreAsync(ScoreCreateModel model)
        {
            // Проверяем, что submission существует
            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == model.SubmissionId);

            if (submission == null) return false;

            // Проверяем валидность числовой оценки
            return model.NumericValue >= 0 && model.NumericValue <= model.MaxPoints;
        }

        public async Task<bool> CanTeacherGradeSubmissionAsync(Guid teacherId, Guid submissionId)
        {
            // Проверяем, что учитель может оценивать эту работу
            // TODO: добавить проверку связи учителя с группой/предметом
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            return submission != null;
        }



        // Helper methods

        private async Task UpdateSubmissionGradeStatus(Guid submissionId)
        {
            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return;

            var hasActiveScore = await _context.Scores
                .AnyAsync(s => s.SubmissionId == submissionId && s.Status == ScoreStatus.Final);

            if (hasActiveScore && submission.Status == SubmissionStatus.Submitted)
            {
                submission.Status = SubmissionStatus.Graded;
                submission.GradedAt = DateTime.UtcNow;
                submission.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private async Task<ScoreResponseModel> MapToResponseModel(Score score)
        {
            if (score.Teacher == null)
            {
                await _context.Entry(score)
                    .Reference(s => s.Teacher)
                    .LoadAsync();
            }

            return new ScoreResponseModel
            {
                Id = score.Id,
                SubmissionId = score.SubmissionId,
                TeacherId = score.TeacherId,
                TeacherName = score.Teacher?.FullName,
                NumericValue = score.NumericValue,
                MaxPoints = score.MaxPoints,
                Feedback = score.Feedback,
                Status = score.Status,
                AwardedAt = score.AwardedAt,
                UpdatedAt = score.UpdatedAt,
                Version = score.Version,
                PreviousVersionId = score.PreviousVersionId,
                PercentageScore = score.GetPercentageScore(),
                DisplayValue = score.GetDisplayValue()
            };
        }

        private static double GetMedian(List<double> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            int count = sorted.Count;
            
            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            else
                return sorted[count / 2];
        }

        private static Dictionary<string, int> GetGradeDistribution(List<Score> scores)
        {
            var distribution = new Dictionary<string, int>
            {
                ["A (90-100%)"] = 0,
                ["B (80-89%)"] = 0,
                ["C (70-79%)"] = 0,
                ["D (60-69%)"] = 0,
                ["F (0-59%)"] = 0
            };

            foreach (var score in scores)
            {
                var percentage = score.GetPercentageScore();
                var grade = percentage switch
                {
                    >= 90 => "A (90-100%)",
                    >= 80 => "B (80-89%)",
                    >= 70 => "C (70-79%)",
                    >= 60 => "D (60-69%)",
                    _ => "F (0-59%)"
                };
                distribution[grade]++;
            }

            return distribution;
        }


    }
}