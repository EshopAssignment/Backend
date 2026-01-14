namespace Application.DTOs.Order;

public sealed record CreateOrderRequestDto(
    IReadOnlyList<CreateOrderItemRequestDto> Items,
    string CartId,
    string Currency = "SEK",
    int ReservationTtlMinutes = 60
);
