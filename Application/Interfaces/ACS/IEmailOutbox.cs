namespace Application.Interfaces.ACS
{
    public interface IEmailOutbox
    {
        Task EnqueueAsync(string to, string subject, string htmlBody, string kind, string? correlationId, CancellationToken ct);
    }
}