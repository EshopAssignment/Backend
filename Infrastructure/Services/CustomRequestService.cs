using Application.DTOs.Order;
using Application.Interfaces;
using Application.Interfaces.ACS;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class CustomRequestService(
    IEmailOutbox emailOutbox,
    IEmailTemplateRenderer emailTemplateRenderer,
    IConfiguration config,
    ILogger<CustomRequestService> logger,
    PallshoppenDbContext dbContext) : ICustomRequestService
{
    private readonly IEmailOutbox _emailOutbox = emailOutbox;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer = emailTemplateRenderer;
    private readonly IConfiguration _config = config;
    private readonly ILogger<CustomRequestService> _logger = logger;
    public async Task<(bool Ok, string? Error)> CreateAsync(CreateCustomRequestFormDto form, CancellationToken ct)
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

            string? attachmentFileName = null;
            string? attachmentBlobPath = null;

            if (form.File is not null)
            {
                attachmentFileName = form.File.FileName;
                attachmentBlobPath = $"custom-requests/{Guid.NewGuid()}-{Path.GetFileName(form.File.FileName)}";
            }

            var entity = new CustomRequest
            {
                Name = form.Name.Trim(),
                Email = form.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(form.Phone) ? null : form.Phone.Trim(),
                Message = form.Message.Trim(),
                AttatchemntName = attachmentFileName,
                AttatchemtBlobPath = attachmentBlobPath
            };

            dbContext.CustomRequest.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var internalHtml = _emailTemplateRenderer.RenderCustomRequestInternal(
                entity.Name,
                entity.Email,
                entity.Phone ?? "",
                entity.Message,
                entity.AttatchemntName);

            var customerHtml = _emailTemplateRenderer.RenderCustomRequestCustomer(entity.Name);

            var internalKind = "custom_request_internal";
            var internalCorrelationId = $"customrequest:{entity.Id}:internal";

            var customerKind = "custom_request_confirmation";
            var customerCorrelationId = $"customrequest:{entity.Id}:customer";

            await _emailOutbox.EnqueueAsync(
                to: internalRecipient,
                subject: $"Ny specialförfrågan från {entity.Name}",
                htmlBody: internalHtml,
                kind: internalKind,
                correlationId: internalCorrelationId,
                ct: ct);

            await _emailOutbox.EnqueueAsync(
                to: entity.Email,
                subject: "Vi har tagit emot din specialförfrågan",
                htmlBody: customerHtml,
                kind: customerKind,
                correlationId: customerCorrelationId,
                ct: ct);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create custom request for {Email}", form.Email);
            return (false, "Något gick fel när förfrågan skulle skickas.");
        }
    }
}