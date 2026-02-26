
namespace Domain.Entities;

public sealed class OutboxMessage
{
    public long Id { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public string CorrelationId { get; set; } = default!;
    public DateTime? PublichedAtUtc { get; set; }
    public int PublishAttempts { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public string? LastError { get; set; }
}
