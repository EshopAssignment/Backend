

using Domain.Enums;

namespace Application.DTOs.Order;

public sealed record MyOrderListItemDto

(    DateTime CreatedAtUtc,
    string OrderNumber,
    OrderStatus OrderStatus,
    decimal GrandTotal,
    string? TrackingUrl,
    string? ReceiptUrl);

