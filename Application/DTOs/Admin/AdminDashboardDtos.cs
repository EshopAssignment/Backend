using Domain.Enums;

namespace Application.DTOs.Admin;

public sealed record AdminDashboardQueryDto(
    string Range,
    DateTime? FromUtc,
    DateTime? ToUtc);

public sealed record AdminDashboardDto(
    AdminDashboardSummaryDto Summary,
    IReadOnlyList<AdminDashboardSeriesPointDto> RevenueSeries,
    IReadOnlyList<AdminTopProductDto> TopProductsByUnits,
    IReadOnlyList<AdminTopProductDto> TopProductsByRevenue,
    IReadOnlyList<AdminOrderStatusCountDto> StatusBreakdown,
    IReadOnlyList<AdminOrderListItemDto> RecentOrders,
    IReadOnlyList<AdminFulfillmentDashboardDto> FulillmentSummary);

public sealed record AdminDashboardSummaryDto(
    decimal Revenue,
    int OrderCount,
    int UnitsSold,
    decimal AverageOrderValue);

public sealed record AdminDashboardSeriesPointDto(
    string Label,
    decimal Revenue,
    int Orders,
    int UnitsSold
    );

public sealed record AdminTopProductDto(
    int ProductId,
    string ProductName,
    string? Sku,
    int UnitsSold,
    decimal Revenue);

public sealed record AdminOrderStatusCountDto(
    OrderStatus OrderStatus,
    int Count);
public sealed record AdminFulfillmentSummaryDto(
    int ReadyCount,
    int OverdueCount,
    int FulfilledTodayCount);