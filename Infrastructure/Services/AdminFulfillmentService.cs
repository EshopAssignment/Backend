using System.Linq.Expressions;
using Application.DTOs;
using Application.DTOs.Admin;
using Application.Interfaces;
using Application.Options;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class AdminFulfillmentService(PallshoppenDbContext dbContext, IOptions<FulfillmentOptions> options) : IAdminFulfillmentService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly int _overdueAfterDays= options.Value.OverdueAfterDays > 0 ? options.Value.OverdueAfterDays : 7;

    public async Task<PagedResultDto<AdminFulfillmentOrderDto>> GetQueueAsync(FulfillmentQueueFilterDto filter, CancellationToken ct = default)
    {
        var normalizedPage = filter.Page < 1 ? DefaultPage : filter.Page;
        var normalizedPageSize = filter.PageSize < 1 ? DefaultPageSize : Math.Min(filter.PageSize, MaxPageSize);

        var utcNow = DateTime.UtcNow;
        var cutoff = utcNow.AddDays(-_overdueAfterDays);

        IQueryable<Order> query = dbContext.Orders
            .AsNoTracking()
            .Where(IsRelevantForFulfillment());

        if(filter.FulfillmentStatus is not null)
        {
            query = query
                .Where(x => x.FulfillmentStatus == filter.FulfillmentStatus.Value);
        }
        if(filter.OverdueOnly)
        {
            query = query
                .Where(x => x.FulfillmentStatus != FulfillmentStatus.Fulfilled && (x.ConfirmedAt ?? x.CreatedAt) <= cutoff);
        }

        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var q = filter.Query.Trim();

            query = query
                .Where(x => x.OrderNumber.Contains(q) ||
                (x.CustomerEmail != null && x.CustomerEmail.Contains(q)) ||
                (x.CustomerFirstName != null && x.CustomerFirstName.Contains(q)) ||
                (x.CustomerLastName != null && x.CustomerLastName.Contains(q)));
        }

        query = query
            .OrderByDescending(x => x.FulfillmentStatus != FulfillmentStatus.Fulfilled && (x.ConfirmedAt ?? x.CreatedAt) <= cutoff)
            .ThenBy(x => x.FulfillmentStatus)
            .ThenBy(x => x.ConfirmedAt ?? x.CreatedAt)
            .ThenBy(x => x.Id);
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new AdminFulfillmentOrderDto(
                x.Id,
                x.OrderNumber,
                x.CustomerFirstName ?? string.Empty,
                x.CustomerLastName ?? string.Empty,
                x.CustomerEmail,
                x.CustomerPhoneNumber,
                x.CreatedAt,
                x.ConfirmedAt,
                x.OrderStatus,
                x.FulfillmentStatus,
                x.FulfillmentStatus != FulfillmentStatus.Fulfilled &&
                (x.ConfirmedAt ?? x.CreatedAt) <= cutoff,
                x.FulfilledAt,
                x.FulfillmentNote,
                x.TrackingNumber,
                x.Currency,
                x.ProductsSubtotal,
                x.ShippingCost,
                x.VatTotal,
                x.GrandTotal
             ))
            .ToListAsync(ct);

        return new PagedResultDto<AdminFulfillmentOrderDto>(
            items,
            normalizedPage,
            normalizedPageSize,
            totalCount);
    }
    public async Task<AdminFulfillmentDashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var utcNow = DateTime.UtcNow;
        var cutoff = utcNow.AddDays(-_overdueAfterDays);
        var startOfTodayUtc = utcNow.Date;

        var baseQuery = dbContext.Orders
            .AsNoTracking()
            .Where(IsRelevantForFulfillment());

        var readyCount = await baseQuery.CountAsync(x => 
        x.FulfillmentStatus != FulfillmentStatus.Fulfilled && (x.ConfirmedAt ?? x.CreatedAt) <= cutoff, ct);

        var overdueCount = await baseQuery.CountAsync(x =>
           x.FulfillmentStatus != FulfillmentStatus.Fulfilled && (x.ConfirmedAt ?? x.CreatedAt) <= cutoff, ct);

        var fulfilledTodayCount = await dbContext.Orders
            .AsNoTracking()
            .CountAsync(x =>
            x.FulfillmentStatus == FulfillmentStatus.Fulfilled &&
            x.FulfilledAt != null &&
            x.FulfilledAt >= startOfTodayUtc, ct);

        var needsAttention = await baseQuery
            .OrderByDescending(x =>
                x.FulfillmentStatus != FulfillmentStatus.Fulfilled &&
                (x.ConfirmedAt ?? x.CreatedAt) <= cutoff)
            .ThenBy(x => x.ConfirmedAt ?? x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(10)
            .Select(x => new AdminFulfillmentOrderDto(
                x.Id,
                x.OrderNumber,
                x.CustomerFirstName ?? string.Empty,
                x.CustomerLastName ?? string.Empty,
                x.CustomerEmail,
                x.CustomerPhoneNumber,
                x.CreatedAt,
                x.ConfirmedAt,
                x.OrderStatus,
                x.FulfillmentStatus,
                x.FulfillmentStatus != FulfillmentStatus.Fulfilled &&
                (x.ConfirmedAt ?? x.CreatedAt) <= cutoff,
                x.FulfilledAt,
                x.FulfillmentNote,
                x.TrackingNumber,
                x.Currency,
                x.ProductsSubtotal,
                x.ShippingCost,
                x.VatTotal,
                x.GrandTotal
            ))
            .ToListAsync(ct);

        return new AdminFulfillmentDashboardDto(
            readyCount,
            overdueCount,
            fulfilledTodayCount,
            needsAttention);
    }
    public async Task<AdminFulfillmentOrderDto?> GetByIdAsync(int orderId, CancellationToken ct = default)
    {
        var utcNow = DateTime.UtcNow;
        var cutoff = utcNow.AddDays(-_overdueAfterDays);

        return await dbContext.Orders
            .AsNoTracking()
            .Where(x => x.Id == orderId)
            .Select(x => new AdminFulfillmentOrderDto(
                x.Id,
                x.OrderNumber,
                x.CustomerFirstName ?? string.Empty,
                x.CustomerLastName ?? string.Empty,
                x.CustomerEmail,
                x.CustomerPhoneNumber,
                x.CreatedAt,
                x.ConfirmedAt,
                x.OrderStatus,
                x.FulfillmentStatus,
                x.FulfillmentStatus != FulfillmentStatus.Fulfilled &&
                (x.ConfirmedAt ?? x.CreatedAt) <= cutoff,
                x.FulfilledAt,
                x.FulfillmentNote,
                x.TrackingNumber,
                x.Currency,
                x.ProductsSubtotal,
                x.ShippingCost,
                x.VatTotal,
                x.GrandTotal
            ))
            .FirstOrDefaultAsync(ct);
    }

    //Actions
    public async Task MarkFulfilledAsync(int orderId, string? note, CancellationToken ct = default)
    {
        var order = await dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId, ct)
            ?? throw new KeyNotFoundException($"Order med id '{orderId}' hittades inte.");

        EnsureOrderCanBeHandled(order);

        order.MarkFulfilled(note);

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task ReopenAsync(int orderId, string? note, CancellationToken ct = default)
    {
        var order = await dbContext.Orders
    .FirstOrDefaultAsync(x => x.Id == orderId, ct)
    ?? throw new KeyNotFoundException($"Order med id '{orderId}' hittades inte.");

        EnsureOrderCanBeHandled(order);

        order.ReopenFulfillment(note);

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task SetFulfillmentNoteAsync(int orderId, string? note, CancellationToken ct = default)
    {
        var order = await dbContext.Orders
    .FirstOrDefaultAsync(x => x.Id == orderId, ct)
    ?? throw new KeyNotFoundException($"Order med id '{orderId}' hittades inte.");

        EnsureOrderCanBeHandled(order);

        order.SetFulfillmentNote(note);

        await dbContext.SaveChangesAsync(ct);
    }

    //Helper/expressons

    private static Expression<Func<Order, bool>> IsRelevantForFulfillment()
    {
        return x =>
            x.OrderStatus == OrderStatus.Confirmed ||
            x.OrderStatus == OrderStatus.Processing;
    }

    private static void EnsureOrderCanBeHandled(Order order)
    {
        if (order.OrderStatus is OrderStatus.Cancelled or OrderStatus.Failed or OrderStatus.Refunded)
        {
            throw new InvalidOperationException(
                $"Order '{order.OrderNumber}' kan inte hanteras i fulfillment när status är '{order.OrderStatus}'.");
        }
    }
}
