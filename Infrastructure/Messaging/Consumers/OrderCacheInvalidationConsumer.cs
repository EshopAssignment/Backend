
using Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Messaging.Consumers;

public sealed class OrderCacheInvalidationConsumer(IDistributedCache cache) : IConsumer<OrderStatusChangedEvent>, IConsumer<OrderTrackingSetEvent>
{
    private readonly IDistributedCache _cache = cache;

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> ctx)
    {
        await _cache.RemoveAsync($"orders:admin:byid:{ctx.Message.OrderId}", ctx.CancellationToken);

        await _cache.SetStringAsync(
            "orders:ver:adminlist",
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            ctx.CancellationToken);
    }

    public async Task Consume(ConsumeContext<OrderTrackingSetEvent> ctx)
    {
        await _cache.RemoveAsync($"orders:admin:byid:{ctx.Message.OrderId}", ctx.CancellationToken);
        await _cache.SetStringAsync(
            "orders:ver:adminlist",
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            ctx.CancellationToken);
    }
}
