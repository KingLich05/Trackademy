using Trackademy.Application.PaymentServices;

namespace Trackademy.Api.BackgroundServices;

public class PendingPaymentBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingPaymentBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public PendingPaymentBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PendingPaymentBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingPaymentBackgroundService запущена. Проверка каждые 6 часов.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Запуск проверки студентов с истекшей оплатой в {Time}", DateTime.Now);
                
                await CreatePendingPayments();
                
                _logger.LogInformation("Проверка завершена в {Time}. Следующая проверка через 6 часов.", DateTime.Now);
                
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании платежей для неоплаченных студентов");
                
                // Подождать 1 час при ошибке перед повторной попыткой
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task CreatePendingPayments()
    {
        using var scope = _serviceProvider.CreateScope();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        
        await paymentService.CreatePendingPaymentsForUnpaidStudentsAsync();
    }
}
