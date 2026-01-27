
using Domain.Enums;

namespace Application.DTOs.Order;

public sealed record OrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPriceExVat,
    decimal LineTotalExVat
    );

public sealed record OrderDetailsDto(
    int OrderId,
    string OrderNumber,
    DateTime CreatedAtUtc,
    string Currency,

    decimal ProductsSubtotal,
    decimal ShippingCost,
    decimal VatTotal,
    decimal GrandTotal,       

    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,

    string? CustomerFirstName,
    string? CustomerLastName,
    string? CustomerEmail,
    string? CustomerPhoneNumber,

    ShippingAddressDto? ShippingAddress,
    ShippingCarrier ShippingCarrier,
    ShippingMethod ShippingMethod,
    string? ServicePointId,

    string? TrackingNumber,
    string? TrackingUrl,

    IReadOnlyList<OrderItemDto> Items,
    int? UserId
    );
