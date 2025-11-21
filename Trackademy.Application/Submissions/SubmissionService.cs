using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Users;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Submissions
{
    public class SubmissionService : ISubmissionService
    {
        private readonly TrackademyDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _uploadPath;
        private readonly long _maxFileSize = 10 * 1024 * 1024;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt" };

        public SubmissionService(TrackademyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _uploadPath = _configuration["FileStorage:SubmissionFiles"] ?? "uploads/submissions";

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<SubmissionResponseModel> CreateOrUpdateAsync(Guid assignmentId, Guid studentId, SubmissionCreateUpdateModel model)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null)
                throw new InvalidOperationException("Задание не найдено");

            var submission = await _context.Submissions
                .Include(s => s.Files)
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

            var now = DateTime.UtcNow;

            if (submission == null)
            {
                submission = new Submission
                {
                    Id = Guid.NewGuid(),
                    AssignmentId = assignmentId,
                    StudentId = studentId,
                    TextContent = model.TextContent,
                    Status = SubmissionStatus.Draft,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await _context.Submissions.AddAsync(submission);
            }
            else
            {
                if (submission.Status != SubmissionStatus.Draft && submission.Status != SubmissionStatus.Returned)
                    throw new InvalidOperationException("Нельзя редактировать отправленную или оцененную работу");

                submission.TextContent = model.TextContent;
                submission.UpdatedAt = now;
                
                if (submission.Status == SubmissionStatus.Returned)
                {
                    submission.Status = SubmissionStatus.Draft;
                }
            }

            if (model.Files != null && model.Files.Any())
            {
                await ProcessFilesAsync(submission, model.Files);
            }

            await _context.SaveChangesAsync();

            await _context.Entry(submission)
                .Reference(s => s.Student)
                .LoadAsync();
            await _context.Entry(submission)
                .Collection(s => s.Files)
                .LoadAsync();
            await _context.Entry(submission)
                .Collection(s => s.Scores)
                .LoadAsync();

            return MapToResponseModel(submission);
        }

        public async Task SubmitForGradingAsync(Guid submissionId, Guid studentId)
        {
            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == submissionId && s.StudentId == studentId);

            if (submission == null)
                throw new UnauthorizedAccessException("Submission не найден или нет доступа");

            if (submission.Status != SubmissionStatus.Draft && submission.Status != SubmissionStatus.Returned)
                throw new InvalidOperationException("Можно отправить только черновик или возвращенную работу");

            // Проверяем, что есть контент или файлы
            var hasFiles = await _context.SubmissionFiles.AnyAsync(f => f.SubmissionId == submissionId);
            if (string.IsNullOrEmpty(submission.TextContent) && !hasFiles)
                throw new InvalidOperationException("Нельзя отправить пустую работу");

            submission.Status = SubmissionStatus.Submitted;
            submission.SubmittedAt = DateTime.UtcNow;
            submission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task GradeSubmissionAsync(Guid submissionId, Guid teacherId, GradeSubmissionModel model)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Group)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new InvalidOperationException("Submission не найден");

            // Проверяем, что учитель ведет занятия в группе этого задания
            var teacherHasGroup = await _context.Set<Domain.Users.Schedule>()
                .AnyAsync(s => s.TeacherId == teacherId && s.GroupId == submission.Assignment.GroupId);

            if (!teacherHasGroup)
                throw new InvalidOperationException("Учитель не ведет занятия в этой группе");

            if (submission.Status != SubmissionStatus.Submitted)
                throw new InvalidOperationException("Можно оценить только отправленную работу");

            // Создаем или обновляем оценку
            var existingScore = await _context.Scores.FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
            if (existingScore != null)
            {
                existingScore.NumericValue = model.Score;
                existingScore.AwardedAt = DateTime.UtcNow;
            }
            else
            {
                var score = new Score
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submissionId,
                    TeacherId = teacherId,
                    NumericValue = model.Score,
                    AwardedAt = DateTime.UtcNow
                };
                _context.Scores.Add(score);
            }
            
            submission.TeacherComment = model.TeacherComment;
            submission.Status = SubmissionStatus.Graded;
            submission.GradedAt = DateTime.UtcNow;
            submission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task ReturnSubmissionAsync(Guid submissionId, Guid teacherId, ReturnSubmissionModel model)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new InvalidOperationException("Submission не найден");

            if (submission.Status != SubmissionStatus.Submitted)
                throw new InvalidOperationException("Можно вернуть только отправленную работу");

            submission.TeacherComment = model.TeacherComment;
            submission.Status = SubmissionStatus.Returned;
            submission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<SubmissionResponseModel>> GetSubmissionsAsync(GetSubmissionsRequest request)
        {
            var query = _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Files)
                .Include(s => s.Scores)
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Group)
                .Where(s => s.Assignment.Group.OrganizationId == request.OrganizationId);

            if (request.StudentId.HasValue)
            {
                query = query.Where(s => s.StudentId == request.StudentId.Value);
            }

            if (request.AssignmentId.HasValue)
            {
                query = query.Where(s => s.AssignmentId == request.AssignmentId.Value);
            }

            if (request.GroupId.HasValue)
            {
                query = query.Where(s => s.Assignment.GroupId == request.GroupId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(s => s.Status == request.Status.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(s => s.SubmittedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(s => s.SubmittedAt <= request.ToDate.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.UpdatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResult<SubmissionResponseModel>
            {
                Items = items.Select(MapToResponseModel).ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<FileDownloadResult> DownloadFileAsync(Guid fileId, Guid userId, string userRole)
        {
            var file = await _context.SubmissionFiles
                .Include(f => f.Submission)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
                throw new FileNotFoundException("Файл не найден");

            // Проверяем доступ
            bool hasAccess = userRole == "Teacher" || file.Submission.StudentId == userId;
            if (!hasAccess)
                throw new UnauthorizedAccessException("Нет доступа к файлу");

            var fullPath = Path.Combine(_uploadPath, file.FilePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Физический файл не найден");

            var content = await File.ReadAllBytesAsync(fullPath);
            
            return new FileDownloadResult
            {
                Content = content,
                ContentType = file.ContentType,
                FileName = file.OriginalFileName
            };
        }

        public async Task DeleteFileAsync(Guid fileId, Guid studentId)
        {
            var file = await _context.SubmissionFiles
                .Include(f => f.Submission)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null || file.Submission.StudentId != studentId)
                throw new UnauthorizedAccessException("Файл не найден или нет доступа");

            if (file.Submission.Status != SubmissionStatus.Draft && file.Submission.Status != SubmissionStatus.Returned)
                throw new InvalidOperationException("Нельзя удалить файл из отправленной или оцененной работы");

            // Удаляем физический файл
            var fullPath = Path.Combine(_uploadPath, file.FilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            // Удаляем запись из БД
            _context.SubmissionFiles.Remove(file);
            
            // Обновляем время изменения submission
            file.Submission.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }

        private async Task ProcessFilesAsync(Submission submission, List<IFormFile> files)
        {
            foreach (var file in files)
            {
                ValidateFile(file);

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var storedFileName = $"{Guid.NewGuid()}{extension}";
                var relativePath = Path.Combine(submission.Id.ToString(), storedFileName);
                var fullPath = Path.Combine(_uploadPath, relativePath);

                var submissionDir = Path.Combine(_uploadPath, submission.Id.ToString());
                if (!Directory.Exists(submissionDir))
                {
                    Directory.CreateDirectory(submissionDir);
                }

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var submissionFile = new SubmissionFile
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    OriginalFileName = file.FileName,
                    StoredFileName = storedFileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    FilePath = relativePath,
                    UploadedAt = DateTime.UtcNow,
                    IsImage = IsImageFile(file.ContentType)
                };

                _context.SubmissionFiles.Add(submissionFile);
            }
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

        private static bool IsImageFile(string contentType)
        {
            return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        private SubmissionResponseModel MapToResponseModel(Submission submission)
        {
            return new SubmissionResponseModel
            {
                Id = submission.Id,
                AssignmentId = submission.AssignmentId,
                StudentId = submission.StudentId,
                StudentName = submission.Student?.FullName,
                TextContent = submission.TextContent,
                Status = submission.Status,
                Score = submission.Scores?.FirstOrDefault()?.NumericValue,
                TeacherComment = submission.TeacherComment,
                CreatedAt = submission.CreatedAt,
                UpdatedAt = submission.UpdatedAt,
                SubmittedAt = submission.SubmittedAt,
                GradedAt = submission.GradedAt,
                Files = submission.Files?.Select(f => new SubmissionFileResponseModel
                {
                    Id = f.Id,
                    OriginalFileName = f.OriginalFileName,
                    ContentType = f.ContentType,
                    FileSize = f.FileSize,
                    UploadedAt = f.UploadedAt,
                    IsImage = f.IsImage,
                    DownloadUrl = $"/api/submission/file/{f.Id}"
                }).ToList() ?? new List<SubmissionFileResponseModel>()
            };
        }
    }
}