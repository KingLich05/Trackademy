using Trackademy.Application.PaymentServices;

namespace Trackademy.Api.BackgroundServices;

public class PaymentBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(1); // Проверка каждый час

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
                await UpdateOverduePayments();
                _logger.LogInformation("Проверка просроченных платежей выполнена в {Time}", DateTime.UtcNow);
                
                await Task.Delay(_period, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении просроченных платежей");
                
                // Подождать меньше времени при ошибке
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
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