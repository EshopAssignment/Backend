
using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<StockStatus>))]

public enum StockStatus
{
    OutOfStock = 0,
    LowStock = 1,
    InStock = 2
}
