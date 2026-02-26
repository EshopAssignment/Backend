using System;
using System.Collections.Generic;
using System.Text;
using Application.Interfaces.ACS;
using Contracts.Events;
using Infrastructure.Messaging.Email;
using Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Outbox;

public class OrderConfirmedEmailConsumer(
    PallshoppenDbContext db,
    IEmailOutbox outbox,
    IEmailTemplateRenderer templateRenderer,
    ILogger<OrderConfirmedEmailConsumer> logger) : IConsumer<OrderConfirmedEvent>
{
    private readonly PallshoppenDbContext _db = db;
    private readonly IEmailOutbox _emailOutbox = outbox;
    private readonly IEmailTemplateRenderer _templateRenderer = templateRenderer;
    private readonly ILogger<OrderConfirmedEmailConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        var msg = context.Message;

        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == msg.OrderId, context.CancellationToken);

        if(order is null)
        {
            _logger.LogWarning("Order not fount for event. orderId={OrderId}", msg.OrderId);
                return;
        }

        var email = order.CustomerEmail?.Trim();
        if(string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("order {OrderNumber} saknar Email, körar inte ordermail", order.OrderNumber);
            return;
        }

        var html = _templateRenderer.RenderOrderConfirmation(
            order.OrderNumber,
            order.ToCustomerName(),
            order.Currency,
            order.GrandTotal,
            order.ToEmailItems());

        var kind = "order_confirmation";

        var correlationId = $"{order.OrderNumber}:{kind}";

        await _emailOutbox.EnqueueAsync(
            to: email,
            subject: $"Orderbekräftelse {order.OrderNumber}",
            htmlBody: html,
            kind: kind,
            correlationId: correlationId,
            ct: context.CancellationToken);
    }
}
