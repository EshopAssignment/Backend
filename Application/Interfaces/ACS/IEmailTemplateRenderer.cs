
namespace Application.Interfaces.ACS;

public interface IEmailTemplateRenderer
{
    string RenderOrderConfirmation(
        string orderNumber,
        string customerName,
        string currency,
        decimal total,
        IEnumerable<(string Name, int Qty, decimal Price)> Items);

    string RenderShippingNotification(
        string orderNumber,
        string trackingUrl
        );

    string RenderEmailVerification(string verifyUrl);
    string RenderWelcomeEmail(string customerName);
    string RenderPassawordReset(string resetUrl);
    string RenderCustomRequestCustomer(
        string customerName);
    string RenderCustomRequestInternal(
        string name,
        string email,
        string phone,
        string message,
        string? fileName);
}
