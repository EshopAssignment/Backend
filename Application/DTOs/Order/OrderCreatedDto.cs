using Domain.Enums;

namespace Application.DTOs.Order;

public sealed record OrderCreatedDto(
    int OrderId,
    string OrderNumber,
    DateTime CreatedAtUtc,
    string Currency,
    decimal ProductsSubtotal,
    decimal ShippingCost,
    decimal TaxTotal,
    decimal GrandTotal,
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,
    ShippingAddressDto ShippingAddress,
    int? UserId
    );
