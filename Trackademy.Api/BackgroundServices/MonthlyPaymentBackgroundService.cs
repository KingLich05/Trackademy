using Trackademy.Application.PaymentServices;

namespace Trackademy.Api.BackgroundServices;

/// <summary>
/// Фоновая служба для автоматического создания ежемесячных платежей
/// Запускается в 00:00 1-го числа каждого месяца
/// </summary>
public class MonthlyPaymentBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonthlyPaymentBackgroundService> _logger;

    public MonthlyPaymentBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MonthlyPaymentBackgroundService> logger)
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
                DateTime nextRun;

                // Если сегодня 1-е число и время до 00:10, то запускаем сегодня
                if (now.Day == 1 && now.Hour == 0 && now.Minute < 10)
                {
                    nextRun = now;
                }
                else
                {
                    // Иначе вычисляем следующее 1-е число месяца в 00:00
                    var nextMonth = now.AddMonths(1);
                    nextRun = new DateTime(nextMonth.Year, nextMonth.Month, 1, 0, 0, 0);
                }

                var delayUntilNextRun = nextRun - now;
                
                _logger.LogInformation(
                    "Следующее создание ежемесячных платежей запланировано на {Time}", 
                    nextRun);
                
                if (delayUntilNextRun.TotalMilliseconds > 0)
                {
                    await Task.Delay(delayUntilNextRun, stoppingToken);
                }
                
                await CreateMonthlyPayments();
                
                _logger.LogInformation(
                    "Ежемесячные платежи успешно созданы в {Time}", 
                    DateTime.Now);
                
                // Ждем 2 минуты, чтобы не запуститься повторно в тот же час
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Приложение останавливается - это нормально, не логируем как ошибку
                _logger.LogInformation("Служба ежемесячных платежей остановлена");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании ежемесячных платежей");
                
                // Подождать час при ошибке перед повторной попыткой
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Служба ежемесячных платежей остановлена во время ожидания");
                    break;
                }
            }
        }
    }

    private async Task CreateMonthlyPayments()
    {
        using var scope = _serviceProvider.CreateScope();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        
        await paymentService.CreateMonthlyPaymentsAsync();
    }
}
