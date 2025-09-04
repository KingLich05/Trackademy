using Microsoft.EntityFrameworkCore;
using Trackademy.Application.GroupServices;
using Trackademy.Application.Helper;
using Trackademy.Application.OrganizationServices;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.BaseCrud;
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
        services.AddSingleton<ExtensionString>();
        
        services.AddAutoMapper(typeof(UserProfile).Assembly);
        return services;
    }
}