using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Trackademy.Api.DI;
using Trackademy.Application.Persistance;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    builder.Services.AddDependencies(builder.Configuration);

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trackademy API", Version = "v1" });
    
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Введите JWT токен как: Bearer {token}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };
        c.AddSecurityDefinition("Bearer", securityScheme);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { securityScheme, Array.Empty<string>() }
        });
    });
    
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminsOnly", p => p.RequireRole("Admin"));
        options.AddPolicy("TeacherOnly", p => p.RequireRole("Teacher"));
        options.AddPolicy("StudentsOnly", p => p.RequireRole("Student"));
    });

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<TrackademyDbContext>();
        await context.Database.MigrateAsync();
    }

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    throw;
}