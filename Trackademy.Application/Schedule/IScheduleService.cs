using Trackademy.Application.Schedule.Model;
using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.Schedule;

public interface IScheduleService
{
    Task<Guid> CreateSchedule(ScheduleAddModel addModel);

    Task<PagedResult<ScheduleViewModel>> GetAllSchedulesAsync(ScheduleRequest scheduleRequest);

    Task<bool> DeleteAsync(Guid id);
    
    Task<Guid> UpdateScheduleAsync(
        Guid id,
        ScheduleUpdateModel addModel);

    Task<ScheduleViewModel?> GetSchedule(Guid id);
}