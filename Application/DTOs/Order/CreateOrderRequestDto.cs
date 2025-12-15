namespace Application.DTOs.Order;

public sealed record CreateOrderRequestDto(
    string CustomerFirstName,
    string CustomerLastName,
    string CustomerEmail,
    string CustomerPhoneNumber,
    ShippingAddressDto ShippingAddress,
    IReadOnlyList<CreateOrderItemRequestDto> Items,
    string CartId,
    string Currency = "SEK",
    decimal? ShippingCost = null,
    int ReservationTtlMinutes = 60
    );
