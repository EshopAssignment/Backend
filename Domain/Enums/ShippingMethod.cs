using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<ShippingMethod>))]
public enum ShippingMethod
{
    None = 0,
    ServicePoint = 1,
    HomeDelivery = 2
}
