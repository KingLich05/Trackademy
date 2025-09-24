using Trackademy.Application.Schedule.Model;

namespace Trackademy.Application.Schedule;

public interface IScheduleService
{
    Task<bool> CreateSchedule(ScheduleAddModel addModel);
}