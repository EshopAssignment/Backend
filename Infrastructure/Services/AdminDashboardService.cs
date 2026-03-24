using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AdminDashboardService(PallshoppenDbContext dbContext) : IAdminDashboardService
{
    private static readonly OrderStatus[] IncludedStatuses =
    [
        OrderStatus.Confirmed,
        OrderStatus.Processing,
        OrderStatus.Shipped,
        OrderStatus.Completed,
        OrderStatus.Refunded
    ];

    public async Task<AdminDashboardDto> GetAsync(AdminDashboardQueryDto query, CancellationToken ct)
    {
        var (fromUtc, toUtc) = ResolveRange(query);

        var baseOrders = dbContext.Orders
            .AsNoTracking()
            .Where(o => IncludedStatuses.Contains(o.OrderStatus))
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc);

        var summaryRaw = await baseOrders
            .Select(o => new
            {
                o.Id,
                o.GrandTotal,
                Units = o.OrderItems.Sum(i => i.Quantity)
            })
            .ToListAsync(ct);

        var summary = new AdminDashboardSummaryDto(
            Revenue: summaryRaw.Sum(x => x.GrandTotal),
            OrderCount: summaryRaw.Count,
            UnitsSold: summaryRaw.Sum(x => x.Units),
            AverageOrderValue: summaryRaw.Count == 0 ? 0 : summaryRaw.Average(x => x.GrandTotal)
        );

        var groupByMonth = ShouldGroupByMonth(fromUtc, toUtc);

        var revenueSeriesRaw = await baseOrders
            .Select(o => new
            {
                o.Id,
                o.CreatedAt,
                o.GrandTotal,
                Units = o.OrderItems.Sum(i => i.Quantity)
            })
            .ToListAsync(ct);

        var revenueSeries = groupByMonth
            ? revenueSeriesRaw
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new AdminDashboardSeriesPointDto(
                    Label: $"{g.Key.Year}-{g.Key.Month:00}",
                    Revenue: g.Sum(x => x.GrandTotal),
                    Orders: g.Count(),
                    UnitsSold: g.Sum(x => x.Units)
                ))
                .ToList()
            : revenueSeriesRaw
                .GroupBy(x => x.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new AdminDashboardSeriesPointDto(
                    Label: g.Key.ToString("yyyy-MM-dd"),
                    Revenue: g.Sum(x => x.GrandTotal),
                    Orders: g.Count(),
                    UnitsSold: g.Sum(x => x.Units)
                ))
                .ToList();

        var topProductsBase = await dbContext.OrderItems
            .AsNoTracking()
            .Where(i => IncludedStatuses.Contains(i.Order.OrderStatus))
            .Where(i => i.Order.CreatedAt >= fromUtc && i.Order.CreatedAt <= toUtc)
            .GroupBy(i => new { i.ProductId, i.ProductName, i.Sku })
            .Select(g => new AdminTopProductDto(
                ProductId: g.Key.ProductId,
                ProductName: g.Key.ProductName,
                Sku: g.Key.Sku,
                UnitsSold: g.Sum(x => x.Quantity),
                Revenue: g.Sum(x => x.LineTotalIncVat)
            ))
            .ToListAsync(ct);

        var topProductsByUnits = topProductsBase
            .OrderByDescending(x => x.UnitsSold)
            .ThenBy(x => x.ProductName)
            .Take(10)
            .ToList();

        var topProductsByRevenue = topProductsBase
            .OrderByDescending(x => x.Revenue)
            .ThenBy(x => x.ProductName)
            .Take(10)
            .ToList();

        var statusBreakdown = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
            .GroupBy(o => o.OrderStatus)
            .Select(g => new AdminOrderStatusCountDto(
                OrderStatus: g.Key,
                Count: g.Count()
            ))
            .OrderBy(x => x.OrderStatus)
            .ToListAsync(ct);

        var recentOrders = await baseOrders
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .Select(o => new AdminOrderListItemDto(
                o.Id,
                o.OrderNumber,
                o.CreatedAt,
                $"{(o.CustomerFirstName ?? "").Trim()} {(o.CustomerLastName ?? "").Trim()}".Trim(),
                o.CustomerEmail ?? "",
                o.OrderStatus,
                o.Payment.Status,
                o.GrandTotal,
                o.Payment.PaymentMethodType ?? ""
            ))
            .ToListAsync(ct);

        return new AdminDashboardDto(
            Summary: summary,
            RevenueSeries: revenueSeries,
            TopProductsByUnits: topProductsByUnits,
            TopProductsByRevenue: topProductsByRevenue,
            StatusBreakdown: statusBreakdown,
            RecentOrders: recentOrders
        );
    }

    private static (DateTime fromUtc, DateTime toUtc) ResolveRange(AdminDashboardQueryDto query)
    {
        var now = DateTime.UtcNow;
        var range = (query.Range ?? "30d").Trim().ToLowerInvariant();

        if (range == "custom" && query.FromUtc.HasValue && query.ToUtc.HasValue)
        {
            var from = DateTime.SpecifyKind(query.FromUtc.Value, DateTimeKind.Utc);
            var to = DateTime.SpecifyKind(query.ToUtc.Value, DateTimeKind.Utc);
            return (from, to);
        }

        return range switch
        {
            "today" => (now.Date, now),
            "7d" => (now.Date.AddDays(-6), now),
            "30d" => (now.Date.AddDays(-29), now),
            "90d" => (now.Date.AddDays(-89), now),
            "1y" => (now.Date.AddYears(-1).AddDays(1), now),
            _ => (now.Date.AddDays(-29), now)
        };
    }

    private static bool ShouldGroupByMonth(DateTime fromUtc, DateTime toUtc)
        => (toUtc - fromUtc).TotalDays > 92;
}