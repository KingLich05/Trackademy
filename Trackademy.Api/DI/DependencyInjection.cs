using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Attendances;
using Trackademy.Application.Dashboard;
using Trackademy.Application.GroupServices;
using Trackademy.Application.Helper;
using Trackademy.Application.Lessons;
using Trackademy.Application.OrganizationServices;
using Trackademy.Application.Persistance;
using Trackademy.Application.RoomServices;
using Trackademy.Application.Schedule;
using Trackademy.Application.Services;
using Trackademy.Application.SubjectServices;
using Trackademy.Application.Users.AutoMapper;
using Trackademy.Application.Users.Interfaces;
using Trackademy.Application.Users.Services;

namespace Trackademy.Api.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TrackademyDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("TrackademyDbContext")));

        services.AddScoped<IUserServices, UserServices>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();

        services.AddSingleton<ExtensionString>();
        
        services.AddAutoMapper(typeof(UserProfile).Assembly);
        return services;
    }
}