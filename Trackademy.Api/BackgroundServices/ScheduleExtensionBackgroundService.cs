using Trackademy.Application.Lessons;

namespace Trackademy.Api.BackgroundServices;

/// <summary>
/// Фоновая служба для автоматического продления расписания
/// Запускается ежедневно в 00:00
/// </summary>
public class ScheduleExtensionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduleExtensionBackgroundService> _logger;

    public ScheduleExtensionBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ScheduleExtensionBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                
                // Вычисляем следующий запуск в 00:00
                var nextRun = now.Date.AddDays(1); // Завтра в 00:00
                
                // Если текущее время до 00:10, запускаем сегодня
                if (now.Hour == 0 && now.Minute < 10)
                {
                    nextRun = now;
                }

                var delayUntilNextRun = nextRun - now;
                
                _logger.LogInformation(
                    "Следующее продление расписания запланировано на {Time}", 
                    nextRun);
                
                if (delayUntilNextRun.TotalMilliseconds > 0)
                {
                    await Task.Delay(delayUntilNextRun, stoppingToken);
                }
                
                await ExtendSchedules();
                
                _logger.LogInformation(
                    "Расписания успешно продлены в {Time}", 
                    DateTime.Now);
                
                // Ждем 2 минуты, чтобы не запуститься повторно
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Приложение останавливается - это нормально, не логируем как ошибку
                _logger.LogInformation("Служба продления расписания остановлена");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при продлении расписаний");
                // Ждем час перед следующей попыткой в случае ошибки
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Служба продления расписания остановлена во время ожидания");
                    break;
                }
            }
        }
    }

    private async Task ExtendSchedules()
    {
        using var scope = _serviceProvider.CreateScope();
        var lessonService = scope.ServiceProvider.GetRequiredService<ILessonService>();
        
        await lessonService.ExtendSchedulesAsync();
    }
}
