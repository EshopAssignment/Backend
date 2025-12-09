namespace Application.DTOs.Order;

public sealed record OrderCreatedDto(
    int OrderId,
    string OrderNumber,
    DateTime OrderDate,
    decimal Total
    );
