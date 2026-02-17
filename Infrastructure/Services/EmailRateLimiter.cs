using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class EmailRateLimiter(IConfiguration config, ILogger<EmailRateLimiter> logger)
{
    
    private readonly bool _enabled = config.GetValue("EmailRateLimit:Enabled", true);
    private readonly int _perMinute = Math.Max(1, config.GetValue("EmailRateLimit:PerMinute", 5));
    private readonly int _perHour = Math.Max(1, config.GetValue("EmailRateLimit:PerHour", 10));

    private readonly object _lock = new();
    private readonly Queue<DateTimeOffset> _minute = new();
    private readonly Queue<DateTimeOffset> _hour = new();

    private readonly ILogger<EmailRateLimiter> _logger = logger;

    public bool TryConsume(out TimeSpan retryAfter)
    {
        retryAfter = TimeSpan.Zero;
        if (!_enabled) return true;

        var now = DateTimeOffset.UtcNow;
        var minCutoff = now.AddMinutes(-1);
        var hourCutoff = now.AddHours(-1);

        lock (_lock)
        {
            while (_minute.Count > 0 && _minute.Peek() < minCutoff) _minute.Dequeue();
            while (_hour.Count > 0 && _hour.Peek() < hourCutoff) _hour.Dequeue();

            if (_minute.Count >= _perMinute)
            {
                retryAfter = (_minute.Peek().AddMinutes(1) - now);
                _logger.LogWarning("Email rate-limit hit: {Count}/{Limit} per minute. RetryAfter={RetryAfterMs}ms",
                    _minute.Count, _perMinute, (int)retryAfter.TotalMilliseconds);
                return false;
            }

            if (_hour.Count >= _perHour)
            {
                retryAfter = (_hour.Peek().AddHours(1) - now);
                _logger.LogWarning("Email rate-limit hit: {Count}/{Limit} per hour. RetryAfter={RetryAfterMs}ms",
                    _hour.Count, _perHour, (int)retryAfter.TotalMilliseconds);
                return false;
            }

            _minute.Enqueue(now);
            _hour.Enqueue(now);
            return true;
        }
    }
}
