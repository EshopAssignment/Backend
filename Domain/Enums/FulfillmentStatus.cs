
using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<FulfillmentStatus>))]

public enum FulfillmentStatus
{
    Unreviewed = 0,
    Ready = 1,
    Fulfilled = 2,
}
