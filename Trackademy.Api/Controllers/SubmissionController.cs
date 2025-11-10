using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Trackademy.Application.Shared.Models;

namespace Trackademy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubmissionController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;
        private readonly ILogger<SubmissionController> _logger;

        public SubmissionController(ISubmissionService submissionService, ILogger<SubmissionController> logger)
        {
            _submissionService = submissionService;
            _logger = logger;
        }

        /// <summary>
        /// Получить submission студента для задания
        /// </summary>
        [HttpGet("assignment/{assignmentId}")]
        public async Task<IActionResult> GetByAssignment(Guid assignmentId)
        {
            var userId = GetCurrentUserId();
            var submission = await _submissionService.GetByAssignmentAndStudentAsync(assignmentId, userId);
            return Ok(submission);
        }

        /// <summary>
        /// Создать или обновить submission (для студентов)
        /// </summary>
        [HttpPost("assignment/{assignmentId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateOrUpdateSubmission(Guid assignmentId, [FromForm] SubmissionCreateUpdateModel model)
        {
            var studentId = GetCurrentUserId();
            
            try
            {
                var submission = await _submissionService.CreateOrUpdateAsync(assignmentId, studentId, model);
                return Ok(submission);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отправить submission на проверку
        /// </summary>
        [HttpPost("{submissionId}/submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitForGrading(Guid submissionId)
        {
            var studentId = GetCurrentUserId();
            
            try
            {
                await _submissionService.SubmitForGradingAsync(submissionId, studentId);
                return Ok(new { message = "Работа отправлена на проверку" });
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
        /// Оценить submission (для учителей)
        /// </summary>
        [HttpPost("{submissionId}/grade")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GradeSubmission(Guid submissionId, [FromBody] GradeSubmissionModel model)
        {
            var teacherId = GetCurrentUserId();
            
            try
            {
                await _submissionService.GradeSubmissionAsync(submissionId, teacherId, model);
                return Ok(new { message = "Оценка выставлена" });
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
        /// Вернуть работу на доработку (для учителей)
        /// </summary>
        [HttpPost("{submissionId}/return")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ReturnSubmission(Guid submissionId, [FromBody] ReturnSubmissionModel model)
        {
            var teacherId = GetCurrentUserId();
            
            try
            {
                await _submissionService.ReturnSubmissionAsync(submissionId, teacherId, model);
                return Ok(new { message = "Работа возвращена на доработку" });
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
        /// Получить все submissions для задания (для учителей)
        /// </summary>
        [HttpGet("assignment/{assignmentId}/all")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetAllSubmissionsForAssignment(Guid assignmentId)
        {
            var submissions = await _submissionService.GetAllByAssignmentAsync(assignmentId);
            return Ok(submissions);
        }

        /// <summary>
        /// Скачать файл из submission
        /// </summary>
        [HttpGet("file/{fileId}")]
        public async Task<IActionResult> DownloadFile(Guid fileId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            try
            {
                var fileResult = await _submissionService.DownloadFileAsync(fileId, userId, userRole);
                return File(fileResult.Content, fileResult.ContentType, fileResult.FileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Удалить файл из submission (только в статусе Draft)
        /// </summary>
        [HttpDelete("file/{fileId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            var studentId = GetCurrentUserId();
            
            try
            {
                await _submissionService.DeleteFileAsync(fileId, studentId);
                return Ok(new { message = "Файл удален" });
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