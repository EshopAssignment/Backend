using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<EmailOutboxStatus>))]

public enum EmailOutboxStatus
{
    Pending = 0,
    Processing = 1,
    Sent = 2,   
    Failed = 3,
}
