using Application.Interfaces.ACS;
using Infrastructure.ACS;
using Infrastructure.Email;

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
    public string RenderWelcomeEmail(string customerName)
        => AuthTemplates.Welcome(customerName);
    public string RenderPassawordReset(string resetUrl)
        => AuthTemplates.PasswordReset(resetUrl);
    public string RenderEmailVerification(string verifyUrl)
    => AuthTemplates.EmailVerification(verifyUrl);
}