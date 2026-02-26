using Application.Interfaces.ACS;
using Domain.Entities.Mail;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Acs;

public sealed class EmailOutbox(PallshoppenDbContext db) : IEmailOutbox
{
    public async Task EnqueueAsync(string to, string subject, string htmlBody, string kind, string? correlationId, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            var exists = await db.EmailOutBox.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == correlationId && x.Kind == kind && x.Status != EmailOutboxStatus.Failed, ct);

            if (exists) return;
        }

        db.EmailOutBox.Add(new EmailOutboxMessage
        {
            To = to.Trim(),
            Subject = subject.Trim(),
            HtmlBody = htmlBody,
            Kind = kind,
            CorrelationId = correlationId,
            Status = EmailOutboxStatus.Pending,
            Attempts = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            NextAttempt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct);
    }
}