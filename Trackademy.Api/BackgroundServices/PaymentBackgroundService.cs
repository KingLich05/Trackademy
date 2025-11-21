using Trackademy.Application.PaymentServices;

namespace Trackademy.Api.BackgroundServices;

public class PaymentBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentBackgroundService> _logger;

    public PaymentBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PaymentBackgroundService> logger)
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
                var next6AM = DateTime.Today.AddDays(1).AddHours(6);
                
                // Если уже прошло 6:00 сегодня, то следующий запуск завтра в 6:00
                if (now.Hour >= 6)
                {
                    next6AM = DateTime.Today.AddDays(1).AddHours(6);
                }
                else
                {
                    // Если еще не 6:00, то запуск сегодня в 6:00
                    next6AM = DateTime.Today.AddHours(6);
                }

                var delayUntil6AM = next6AM - now;
                
                _logger.LogInformation("Следующая проверка просроченных платежей запланирована на {Time}", next6AM);
                
                await Task.Delay(delayUntil6AM, stoppingToken);
                
                await UpdateOverduePayments();
                _logger.LogInformation("Проверка просроченных платежей выполнена в {Time}", DateTime.Now);
            }
            catch (OperationCanceledException)
            {
                // Приложение останавливается - это нормально, не логируем как ошибку
                _logger.LogInformation("Служба проверки просроченных платежей остановлена");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении просроченных платежей");
                
                // Подождать час при ошибке перед повторной попыткой
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Служба проверки просроченных платежей остановлена во время ожидания");
                    break;
                }
            }
        }
    }

    private async Task UpdateOverduePayments()
    {
        using var scope = _serviceProvider.CreateScope();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        
        await paymentService.UpdateOverduePaymentsAsync();
    }
}