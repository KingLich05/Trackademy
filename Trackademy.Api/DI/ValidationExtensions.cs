using FluentValidation;
using FluentValidation.AspNetCore;
using Trackademy.Application.Schedule;

namespace Trackademy.Api.DI;

public static class ValidationExtensions
{
    /// <summary>
    /// Регистрирует FluentValidation и все валидаторы из сборок приложения.
    /// </summary>
    public static IServiceCollection AddAppValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
      
        services.AddValidatorsFromAssembly(typeof(ScheduleAddModelValidator).Assembly);
        
        return services;
    }
}