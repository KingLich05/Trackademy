using Trackademy.Application.Schedule.Model;

namespace Trackademy.Application.Schedule;

public interface IScheduleService
{
    Task<bool> CreateSchedule(ScheduleAddModel addModel);

    Task<List<ScheduleViewModel>> GetAllSchedulesAsync(ScheduleRequest scheduleRequest);
    
    Task<List<LessonViewModel>> GetAllLessons(LessonRequest lessonRequest);
    
    Task DeleteAsync(Guid id);
}