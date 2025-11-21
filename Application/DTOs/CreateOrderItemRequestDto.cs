

namespace Application.DTOs;

public sealed record CreateOrderItemRequestDto(
    int ProductId,
    int Quantity
    );

