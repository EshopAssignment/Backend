using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<VatRate>))]

public enum VatRate
{
    Vat6 = 6,
    Vat12 = 12,
    Vat25 = 25
}
