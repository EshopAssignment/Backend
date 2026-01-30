using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public sealed class StockReservationDeleteService(IServiceScopeFactory scopeFactory, ILogger<StockReservationDeleteService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jitter = TimeSpan.FromSeconds(Random.Shared.Next(0, 15));

        try { await Task.Delay(jitter, stoppingToken); }
        catch{ }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var inventory = scope.ServiceProvider.GetRequiredService<IInventoryService>();

                var released = await inventory.ReleaseExpiredAsync(stoppingToken);

                if (released > 0)
                {
                    logger.LogInformation("Released {Count} expired stock reservations", released);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                //generic shutdown
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed releasing expired reservations");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException) { }
        }
    }
}
