using System.Text.Json;
using Domain.Entities;

namespace Infrastructure.Messaging.Outbox;

public static class OutboxFactory
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public static OutboxMessage Create<T>(T evt, string correlationId)
        => new()
        {
            Type = typeof(T).FullName ?? typeof(T).Name,
            Payload = JsonSerializer.Serialize(evt, JsonOpts),
            CorrelationId = correlationId,
            CreatedAtUtc = DateTime.UtcNow,
        };

    public static T Deserialize<T>(OutboxMessage msg)
        => JsonSerializer.Deserialize<T>(msg.Payload, JsonOpts)
           ?? throw new InvalidOperationException($"Could not deserialize {msg.Type}");
}