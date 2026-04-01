
using Domain.Enums;

namespace Application.DTOs.Admin;

public sealed record AdminFulfillmentOrderDto(
    int Id,
    string OrderNumber,
    string CustomerFirstName,
    string CustomerLastName,
    string? CustomerEmail,
    string? CustomerPhoneNumber,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    OrderStatus OrderStatus,
    FulfillmentStatus FulfillmentStatus,
    bool IsOverdue,
    DateTime? FulfilledAt,
    string? FulfillmentNote,
    string? TrackingNumber,
    string Currency,
    decimal ProductsSubtotal,
    decimal ShippingCost,
    decimal VatTotal,
    decimal GrandTotal);

public sealed record AdminFulfillmentDashboardDto(
    int ReadyCount,
    int OverdueCount,
    int FulfilledTodayCount,
    IReadOnlyList<AdminFulfillmentOrderDto> NeedsAttention
    );

public sealed record FulfillmentQueueFilterDto(
    FulfillmentStatus? FulfillmentStatus,
    bool OverdueOnly,
    string? Query,
    int Page = 1,
    int PageSize = 20
);
public sealed record MarkOrderFulfillmentRequest(
    string? Note);

public sealed record ReopenFulfillmentRequest(
    string? Note);

public sealed record SetFulfillmentNoteRequest(
    string? Note
);
