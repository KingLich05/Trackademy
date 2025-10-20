using Trackademy.Application.Lessons.Models;
using Trackademy.Application.Schedule.Model;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.Lessons;

public interface ILessonService
{
    Task<LessonViewModel> GetLessonByIdAsync(Guid id);

    Task<List<LessonViewModel>> GetLessonsByScheduleAsync(Guid scheduleId, DateOnly? fromDate = null, DateOnly? toDate = null);
    
    Task<PagedResult<LessonViewModel>> GetLessonsByScheduleAsync(GetLessonsByScheduleRequest request);
    
    Task<bool> UpdateLessonStatusAsync(Guid lessonId, LessonStatus newStatus, string? note = null);
    
    Task<bool> RescheduleLessonAsync(Guid lessonId, LessonRescheduleModel model);

    Task<Guid> CreateCustomLessonAsync(LessonCustomAddModel model);
}