
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<ProductCondition>))]

public enum ProductCondition
{
    [Description("Ny")] New = 1,
    [Description("Begangnad")] Used = 2,
    [Description("Upprustad")] Refurbished = 3
}
