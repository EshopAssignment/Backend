
namespace Application.DTOs.Shipping;

public sealed record SetShippingSelectionDto(
    string Carrier,
    string Method,
    decimal ShippingCost,
    string? ServicePointId);
