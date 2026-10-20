namespace BlackCode.Web.Services;

public class PendingOrderExpirationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<PendingOrderExpirationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var orders = scope.ServiceProvider.GetRequiredService<OrderService>();
                var count = await orders.ExpirePendingOrdersAsync();
                if (count > 0)
                    logger.LogInformation("Se cancelaron {Count} pedidos pendientes con el plazo vencido.", count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "No se pudieron revisar los pedidos pendientes.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
