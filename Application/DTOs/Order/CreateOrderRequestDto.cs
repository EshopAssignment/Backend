namespace Application.DTOs.Order;

public sealed record CreateOrderRequestDto(
    string CustomerFirstName,
    string CustomerLastName,
    string CustomerEmail,
    string CustomerPhoneNumber,
    ShippingAddressDto ShippingAddress,
    IReadOnlyList<CreateOrderItemRequestDto> Items,
    string Currency = "SEK",
    decimal? ShippingCost = null
    );
