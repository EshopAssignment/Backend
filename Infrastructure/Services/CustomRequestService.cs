using Application.DTOs.Order;
using Application.Interfaces;
using Application.Interfaces.ACS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public sealed class CustomRequestService(
    IEmailOutbox emailOutbox,
    IEmailTemplateRenderer emailTemplateRenderer,
    IConfiguration config,
    ILogger<CustomRequestService> logger) : ICustomRequestService
{
    private readonly IEmailOutbox _emailOutbox = emailOutbox;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer = emailTemplateRenderer;
    private readonly IConfiguration _config = config;
    private readonly ILogger<CustomRequestService> _logger = logger;

    public async Task<(bool Ok, string? Error)> CreateAsync(CreateCustomRequestForm form, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(form.Name))
                return (false, "Namn måste fyllas i.");

            if (string.IsNullOrWhiteSpace(form.Email))
                return (false, "E-post måste fyllas i.");

            if (string.IsNullOrWhiteSpace(form.Message))
                return (false, "Beskrivning måste fyllas i.");

            if (form.File is not null && form.File.Length > 10_000_000)
                return (false, "Filen är för stor. Max 10 MB.");

            var internalRecipient = _config["CustomRequests:RecipientEmail"];
            if (string.IsNullOrWhiteSpace(internalRecipient))
                return (false, "Ingen mottagare är konfigurerad för specialförfrågningar.");

            var internalHtml = _emailTemplateRenderer.RenderCustomRequestInternal(
                form.Name,
                form.Email,
                form.Phone,
                form.Message,
                form.File?.FileName);

            var customerHtml = _emailTemplateRenderer.RenderCustomRequestCustomer(form.Name);

            var internalKind = "custom_request_internal";
            var internalCorrelationId = $"{form.Email.Trim().ToLowerInvariant()}:{internalKind}:{DateTime.UtcNow:yyyyMMddHHmmss}";

            var customerKind = "custom_request_confirmation";
            var customerCorrelationId = $"{form.Email.Trim().ToLowerInvariant()}:{customerKind}:{DateTime.UtcNow:yyyyMMddHHmmss}";

            await _emailOutbox.EnqueueAsync(
                to: internalRecipient,
                subject: $"Ny specialförfrågan från {form.Name}",
                htmlBody: internalHtml,
                kind: internalKind,
                correlationId: internalCorrelationId,
                ct: ct);

            await _emailOutbox.EnqueueAsync(
                to: form.Email.Trim(),
                subject: "Vi har tagit emot din specialförfrågan",
                htmlBody: customerHtml,
                kind: customerKind,
                correlationId: customerCorrelationId,
                ct: ct);

            if (form.File is not null)
            {
                _logger.LogInformation(
                    "Custom request received with file {FileName}, but attachment sending/storage is not implemented yet.",
                    form.File.FileName);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create custom request for {Email}", form.Email);
            return (false, "Något gick fel när förfrågan skulle skickas.");
        }
    }
}