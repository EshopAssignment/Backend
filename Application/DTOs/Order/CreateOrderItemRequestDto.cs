namespace Application.DTOs.Order;

public sealed record CreateOrderItemRequestDto(
    int ProductId,
    int Quantity
    );

