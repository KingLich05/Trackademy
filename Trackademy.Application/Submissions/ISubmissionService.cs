namespace Trackademy.Application.Shared.Models
{
    public interface ISubmissionService
    {
        Task<SubmissionResponseModel> CreateOrUpdateAsync(Guid assignmentId, Guid studentId, SubmissionCreateUpdateModel model);
        Task SubmitForGradingAsync(Guid submissionId, Guid studentId);
        Task GradeSubmissionAsync(Guid submissionId, Guid teacherId, GradeSubmissionModel model);
        Task ReturnSubmissionAsync(Guid submissionId, Guid teacherId, ReturnSubmissionModel model);
        Task<PagedResult<SubmissionResponseModel>> GetSubmissionsAsync(GetSubmissionsRequest request);
        Task<SubmissionResponseModel> GetByIdAsync(Guid submissionId, Guid userId);
        Task<FileDownloadResult> DownloadFileAsync(Guid fileId, Guid userId, string userRole);
        Task DeleteFileAsync(Guid fileId, Guid studentId);
    }
}