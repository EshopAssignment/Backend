
using Domain.Enums;

namespace Application.DTOs.Admin;

public sealed record AdminFulfillmentOrderDto(
    int Id,
    string OrderNumber,
    string CustomerName,
    string Email,
    DateTime CreatedAtUtc,
    DateTime? ConfirmedAtUtc,
    OrderStatus OrderStatus,
    FulfillmentStatus FulfillmentStatus,
    bool IsOverdue,
    DateTime? FulFilledAtUtc,
    decimal GrandTotal);

public sealed record AdminFulfillmentDashboardDto(
    int ReadyCount,
    int OverdueCount,
    int FulfilledTodayCount,
    IReadOnlyList<AdminFulfillmentOrderDto> NeedAttention
    );

public sealed record MarkOrderFulfillmentRequest(
    string? Note);

public sealed record ReopenFulfillmentRequest(
    string? Note);
