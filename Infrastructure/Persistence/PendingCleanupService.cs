using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public sealed class PendingCleanupService(IServiceScopeFactory scopeFactory, ILogger<PendingCleanupService> log) : BackgroundService
{
    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);


    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), ct);
        
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<PallshoppenDbContext>();
                var inventroy = scope.ServiceProvider.GetService<Application.Interfaces.IInventoryService>();
                var cutoff = DateTime.UtcNow - MaxAge;

                var doomed = await db.Orders
                    .Where(o => o.CreatedAt < cutoff)
                    .Where(o => o.Payment.PaymentIntentId == null)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.CartId
                    })
                    .ToListAsync(ct);

                if (doomed.Count > 0)
                {
                    var ids = doomed.Select(x => x.Id).ToList();
                    var orderNumbers = string.Join(", ", doomed.Select(x => x.OrderNumber));

                    var deleted = await db.Orders
                        .Where(o => ids.Contains(o.Id))
                        .ExecuteDeleteAsync(ct);

                    log.LogWarning(
                         "Cleanup: deleted {Deleted} orders older than {Age} with no PaymentIntentId. Orders: {Orders}",
                         deleted, MaxAge, orderNumbers
                         );
                    if (inventroy is not null)
                        await inventroy.ReleaseExpiredAsync(ct);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Cleanup failed");
            }


            await Task.Delay(Interval, ct);
        }
    }
}
