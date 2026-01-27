

using Domain.Enums;

namespace Application.DTOs.Admin;

public sealed record AdminOrderListItemDto(
    int Id,
    string OrderNumber,
    DateTime CreatedAtUtc,
    string CustomerName,
    string CustomerEmail,
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,
    decimal GrandTotal,      
    string PaymentMethod
);
public sealed record AdminOrderItemDto(
    int ProductId,
    string Sku,
    string ProductName,

    decimal UnitPriceExVat,
    int VatRatePercent,
    decimal UnitVatAmount,
    decimal UnitPriceIncVat,

    int Quantity,

    decimal LineTotalExVat,
    decimal LineTotalVat,
    decimal LineTotalIncVat
);
public sealed record AdminOrderDetailsDto(
    int Id,
    string OrderNumber,
    DateTime CreatedAtUtc,

    string CustomerFirstName,
    string CustomerLastName,
    string CustomerEmail,
    string CustomerPhoneNumber,

    string ShippingStreet,
    string ShippingPostalCode,
    string ShippingCity,
    string ShippingCountry,

    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,
    string PaymentMethod,
    string? PaymentIntentId,

    string Currency,

    decimal ProductsSubtotal, 
    decimal ShippingCost,     
    decimal VatTotal,         
    decimal GrandTotal,      

    string? TrackingNumber,
    string? TrackingUrl,

    IReadOnlyList<AdminOrderItemDto> Items
);

public sealed record AdminUpdateOrderStatusRequest(OrderStatus OrderStatus);

public sealed record AdminSetTrackingRequest(
    string TrackingNumber,
    bool MarkAsShipped = true);

