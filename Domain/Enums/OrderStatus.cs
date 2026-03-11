using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<OrderStatus>))]

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Completed = 4,
    Cancelled = 5,
    Failed = 6,
    Refunded = 7
}

