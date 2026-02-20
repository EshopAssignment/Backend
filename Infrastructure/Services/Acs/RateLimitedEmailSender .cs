using Application.Interfaces.ACS;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Acs;
public sealed class RateLimitedEmailSender(IEmailSender inner, EmailRateLimiter limiter, ILogger<RateLimitedEmailSender> logger) : IEmailSender
{
    private readonly IEmailSender _inner = inner;
    private readonly EmailRateLimiter _limiter = limiter;
    private readonly ILogger<RateLimitedEmailSender> _logger = logger;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (!_limiter.TryConsume(out var retryAfter))
        {
            throw new InvalidOperationException(
                $"Email rate limit reached. Retry after ~{Math.Ceiling(retryAfter.TotalSeconds)}s. or check Azure resource group");
        }

        await _inner.SendAsync(to, subject, htmlBody, ct);
        _logger.LogInformation("Email allowed by rate limiter: {To} ({Subject})", to, subject);
    }
}
