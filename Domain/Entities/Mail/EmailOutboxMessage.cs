using Domain.Enums;

namespace Domain.Entities.Mail;

public sealed class EmailOutboxMessage
{
    public long Id { get; set; }
    public string To { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string HtmlBody { get; set; } = default!;

    public string Kind { get; set; } = "generic";
    public string? CorrelationId { get; set; }

    public EmailOutboxStatus Status { get; set; } = EmailOutboxStatus.Pending;

    public int Attempts { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? LastAttempt { get; set; }
    public DateTimeOffset? NextAttempt { get; set; }
    public DateTimeOffset SentAt { get; set; }

    public string? LastError { get; set; }
}
