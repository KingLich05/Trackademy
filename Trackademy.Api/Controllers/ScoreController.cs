using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trackademy.Application.Scores;
using Trackademy.Application.Shared.Models;

namespace Trackademy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScoreController : ControllerBase
    {
        private readonly IScoreService _scoreService;
        private readonly ILogger<ScoreController> _logger;

        public ScoreController(IScoreService scoreService, ILogger<ScoreController> logger)
        {
            _scoreService = scoreService;
            _logger = logger;
        }

        /// <summary>
        /// Создать оценку для submission (только для учителей)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateScore([FromBody] ScoreCreateModel model)
        {
            var teacherId = GetCurrentUserId();
            
            try
            {
                var result = await _scoreService.CreateScoreAsync(model, teacherId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Обновить существующую оценку
        /// </summary>
        [HttpPut("{scoreId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateScore(Guid scoreId, [FromBody] ScoreUpdateModel model)
        {
            var teacherId = GetCurrentUserId();
            
            try
            {
                var result = await _scoreService.UpdateScoreAsync(scoreId, model, teacherId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Удалить (отменить) оценку
        /// </summary>
        [HttpDelete("{scoreId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteScore(Guid scoreId)
        {
            var teacherId = GetCurrentUserId();
            
            try
            {
                await _scoreService.DeleteScoreAsync(scoreId, teacherId);
                return Ok(new { message = "Оценка отменена" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получить оценку по ID
        /// </summary>
        [HttpGet("{scoreId}")]
        public async Task<IActionResult> GetScore(Guid scoreId)
        {
            var score = await _scoreService.GetScoreByIdAsync(scoreId);
            
            if (score == null)
                return NotFound(new { message = "Оценка не найдена" });
            
            return Ok(score);
        }

        /// <summary>
        /// Получить все оценки для submission
        /// </summary>
        [HttpGet("submission/{submissionId}")]
        public async Task<IActionResult> GetScoresBySubmission(Guid submissionId)
        {
            var scores = await _scoreService.GetScoresBySubmissionAsync(submissionId);
            return Ok(scores);
        }

        /// <summary>
        /// Получить актуальную оценку для submission
        /// </summary>
        [HttpGet("submission/{submissionId}/latest")]
        public async Task<IActionResult> GetLatestScore(Guid submissionId)
        {
            var score = await _scoreService.GetLatestScoreForSubmissionAsync(submissionId);
            
            if (score == null)
                return NotFound(new { message = "Оценка не найдена" });
            
            return Ok(score);
        }

        /// <summary>
        /// Получить историю изменений оценок для submission
        /// </summary>
        [HttpGet("submission/{submissionId}/history")]
        public async Task<IActionResult> GetScoreHistory(Guid submissionId)
        {
            var history = await _scoreService.GetScoreHistoryAsync(submissionId);
            return Ok(history);
        }

        /// <summary>
        /// Получить все оценки по заданию (для учителей)
        /// </summary>
        [HttpGet("assignment/{assignmentId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetScoresByAssignment(Guid assignmentId)
        {
            var scores = await _scoreService.GetScoresByAssignmentAsync(assignmentId);
            return Ok(scores);
        }

        /// <summary>
        /// Получить статистику по заданию
        /// </summary>
        [HttpGet("assignment/{assignmentId}/statistics")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetAssignmentStatistics(Guid assignmentId)
        {
            var statistics = await _scoreService.GetAssignmentStatisticsAsync(assignmentId);
            return Ok(statistics);
        }

        /// <summary>
        /// Получить оценки студента
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetScoresByStudent(Guid studentId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            // Студент может смотреть только свои оценки
            if (userRole == "Student" && studentId != userId)
                return Forbid("Можно просматривать только свои оценки");
            
            var scores = await _scoreService.GetScoresByStudentAsync(studentId);
            return Ok(scores);
        }

        /// <summary>
        /// Получить средние оценки студентов группы
        /// </summary>
        [HttpGet("group/{groupId}/averages")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetStudentAverages(
            Guid groupId, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null)
        {
            var averages = await _scoreService.GetStudentAveragesAsync(groupId, fromDate, toDate);
            return Ok(averages);
        }

        /// <summary>
        /// Массовое выставление оценок
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateBulkScores([FromBody] BulkScoreModel model)
        {
            var teacherId = GetCurrentUserId();
            
            try
            {
                var results = await _scoreService.CreateBulkScoresAsync(model, teacherId);
                return Ok(new { 
                    message = $"Создано {results.Count} из {model.Scores.Count} оценок",
                    scores = results 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        /// <summary>
        /// Пересчитать оценки по заданию
        /// </summary>
        [HttpPost("assignment/{assignmentId}/recalculate")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> RecalculateScores(Guid assignmentId)
        {
            try
            {
                await _scoreService.RecalculateScoresAsync(assignmentId);
                return Ok(new { message = "Оценки пересчитаны" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
}