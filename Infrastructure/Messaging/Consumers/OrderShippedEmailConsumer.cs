
using Application.Interfaces.ACS;
using Contracts.Events;
using Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

public class OrderShippedEmailConsumer(
    PallshoppenDbContext dbContext,
    IEmailOutbox emailOutbox,
    IEmailTemplateRenderer emailTemplateRenderer,
    ILogger<OrderShippedEmailConsumer> logger) : IConsumer<OrderStatusChangedEvent>, IConsumer<OrderTrackingSetEvent>
{
    private readonly PallshoppenDbContext _dbContext = dbContext;
    private readonly IEmailOutbox _emailOutbox = emailOutbox;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer = emailTemplateRenderer;
    private readonly ILogger<OrderShippedEmailConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        if (!string.Equals(context.Message.ToStatus, "Shipped", StringComparison.OrdinalIgnoreCase))
            return;

        await EnqueueShippedEmail(context.Message.OrderId, context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<OrderTrackingSetEvent> context)
    {
        if (!context.Message.MarkedAsShipped)
            return;
        
        await EnqueueShippedEmail(context.Message.OrderId, context.CancellationToken);
    }

    private async Task EnqueueShippedEmail(int orderId, CancellationToken ct)
    {
        var o = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderId, ct);
        if (o is null) return;

        var email = o.CustomerEmail?.Trim();
        if(string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Order {OrderId} saknar CustomerEmail, skippar mail(shipped)", orderId);
            return;
        }

        var trackingNumber = o.TrackingNumber?.Trim();
        var trackingUrl = string.IsNullOrEmpty(trackingNumber)
            ? "https://tracking.postnord.com/"
            : $"https://tracking.postnord.com/?id={Uri.EscapeDataString(trackingNumber)}";
        var html = _emailTemplateRenderer.RenderShippingNotification(o.OrderNumber, trackingUrl);

        const string kind = "order_shipped";
        var correlationId = $"{o.OrderNumber}:{kind}";

        await _emailOutbox.EnqueueAsync(
            to: email,
            subject: $"Din order {o.OrderNumber} är skickad",
            htmlBody: html,
            kind: kind,
            correlationId: correlationId,
            ct: ct);
    }

}
