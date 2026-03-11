using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<StockReservationStatus>))]

public enum StockReservationStatus
{
    Active = 1,
    Confirmed = 2,
    Released = 3,
}
