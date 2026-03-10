using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<ShippingCarrier>))]
public enum ShippingCarrier
{
    None = 0,
    PostNord =1,
}
