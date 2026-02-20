using Application.Interfaces.ACS;
using Infrastructure.ACS;

namespace Infrastructure.Services.Acs;

public sealed class EmailTemplateRenderer : IEmailTemplateRenderer
{
    public string RenderOrderConfirmation(
        string orderNumber,
        string customerName,
        string currency,
        decimal total,
        IEnumerable<(string Name, int Qty, decimal Price)> items)
        => OrderTemplates.OrderConfirmation(orderNumber, customerName, currency, total, items);

    public string RenderShippingNotification(
        string orderNumber,
        string trackingUrl)
        => OrderTemplates.ShippingNotification(orderNumber, trackingUrl);
}