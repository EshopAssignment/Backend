
using System.Reflection.Metadata;
using Contracts.Events;
using Infrastructure.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.BackgroundServices;

public sealed class OutboxPublisherService(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishBatch(stoppingToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Publisher crashed loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task PublishBatch(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PallshoppenDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var batch = await db.OutboxMessages
            .Where(x => x.PublichedAtUtc == null)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(25)
            .ToListAsync(ct);

        if (batch.Count == 0) return;

        foreach(var msg in batch)
        {
            msg.LastAttemptUtc = DateTime.UtcNow;
            msg.PublishAttempts++;

            try
            {
                if (msg.Type.EndsWith(nameof(OrderCreatedEvent)))
                {
                    var evt = OutboxFactory.Deserialize<OrderCreatedEvent>(msg);
                    await bus.Publish(evt, ct);
                }
                else if (msg.Type.EndsWith(nameof(OrderConfirmedEvent)))
                {
                    var evt = OutboxFactory.Deserialize<OrderConfirmedEvent>(msg);
                    await bus.Publish(evt, ct);
                }
                else
                {
                    throw new InvalidOperationException($"Unknown outbox message type: {msg.Type}");
                }

                msg.PublichedAtUtc = DateTime.UtcNow;
                msg.LastError = null;
            }
            catch (Exception ex)
            {
                msg.LastError = ex.Message;
                _logger.LogWarning(ex, "Faield To publish outbox message {Id} type = {Type}", msg.Id, msg.Type);
            }
        }
        await db.SaveChangesAsync(ct);
    }
}
