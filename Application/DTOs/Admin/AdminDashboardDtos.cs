using Domain.Enums;

namespace Application.DTOs.Admin;

public sealed record AdminDashboardQueryDto(
    string Range,
    DateTime? FromUtc,
    DateTime? ToUtc);

public sealed record AdminDashboardDto(
    AdminDashboardSummaryDto Summary,
    IReadOnlyList<AdminDashboardSeriesPointDto> RevenueSeries,
    IReadOnlyList<AdminTopProductDto> TopProductByUnits,
    IReadOnlyList<AdminTopProductDto> TopProductByRevenue,
    IReadOnlyList<AdminOrderStatusCountDto> StatusBreakdown,
    IReadOnlyList<AdminOrderListItemDto> RecentOrders);

public sealed record AdminDashboardSummaryDto(
    decimal Revenue,
    int OrderCount,
    int UnitsSold,
    decimal AvarageOrderValue);

public sealed record AdminDashboardSeriesPointDto(
    string Lable,
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