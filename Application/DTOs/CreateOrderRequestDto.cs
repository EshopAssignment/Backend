

namespace Application.DTOs;

public sealed record CreateOrderRequestDto(
    string CustomerFirstName,
    string CustomerLastName,
    string CustomerEmail,
    string CustomerPhoneNumber,
    string ShippingCity,
    string ShippingStreet,
    string ShippingPostalCode,
    string ShippingCountry,
    IReadOnlyList<CreateOrderItemRequestDto> Items
    );
