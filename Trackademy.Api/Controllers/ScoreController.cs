using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trackademy.Application.Scores;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers
{
    /// <summary>
    /// Score API - используется только для АНАЛИТИКИ и СТАТИСТИКИ оценок.
    /// Для выставления оценок используй Submission API: POST /api/Submission/{id}/grade
    /// </summary>
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
        /// Получить статистику по заданию (средний балл, количество оценок и т.д.)
        /// </summary>
        [HttpGet("assignment/{assignmentId}/statistics")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetAssignmentStatistics(Guid assignmentId)
        {
            var statistics = await _scoreService.GetAssignmentStatisticsAsync(assignmentId);
            return Ok(statistics);
        }

        /// <summary>
        /// Получить все оценки студента
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
        /// Получить средние оценки студентов группы (для аналитики)
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