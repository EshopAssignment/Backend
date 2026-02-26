using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.BackgroundServices;
//chatGpt copy-paste.
public sealed class StockReservationDeleteService(IServiceScopeFactory scopeFactory, ILogger<StockReservationDeleteService> logger) : BackgroundService
{
    private static readonly TimeSpan BaseInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan MinBackoff = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SafeDelay(TimeSpan.FromSeconds(Random.Shared.Next(0, 15)), stoppingToken);

        var consecutiveFailures = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var inventory = scope.ServiceProvider.GetRequiredService<IInventoryService>();

                var released = await inventory.ReleaseExpiredAsync(stoppingToken);

                consecutiveFailures = 0;

                if (released > 0)
                {
                    logger.LogInformation("Released {Count} expired stock reservations", released);
                }
                else
                {
                    logger.LogDebug("No expired stock reservations to release");
                }

                await SafeDelay(Jitter(BaseInterval), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                consecutiveFailures++;

                var backoff = ComputeBackoff(consecutiveFailures);
                logger.LogError(
                    ex,
                    "Failed releasing expired reservations (attempt {Attempt}). Backing off for {Backoff}.",
                    consecutiveFailures,
                    backoff);

                await SafeDelay(Jitter(backoff), stoppingToken);
            }
        }
    }

    private static TimeSpan ComputeBackoff(int failures)
    {
 
        var seconds = MinBackoff.TotalSeconds * Math.Pow(2, Math.Max(0, failures - 1));
        var backoff = TimeSpan.FromSeconds(seconds);

        if (backoff > MaxBackoff) backoff = MaxBackoff;
        return backoff;
    }

    private static TimeSpan Jitter(TimeSpan t)
    {
        var jitterFactor = 0.9 + (Random.Shared.NextDouble() * 0.2);
        var ms = Math.Max(0, t.TotalMilliseconds * jitterFactor);
        return TimeSpan.FromMilliseconds(ms);
    }

    private static async Task SafeDelay(TimeSpan delay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
    }
}
