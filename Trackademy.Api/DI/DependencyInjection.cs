using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
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

        return services;
    }
}