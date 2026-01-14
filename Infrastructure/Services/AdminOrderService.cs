
using Application.DTOs.Admin;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AdminOrderService(PallshoppenDbContext dbContext) : IAdminOrderService
{
    public async Task<PagedResult<AdminOrderListItemDto>> GetAllAsync(int page, int pageSize, string? query, string? status, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = dbContext.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();

            q = q.Where(o =>
                o.OrderNumber.Contains(term) ||
                (o.CustomerFirstName != null && o.CustomerFirstName.Contains(term)) ||
                (o.CustomerLastName != null && o.CustomerLastName.Contains(term)) ||
                (o.CustomerEmail != null && o.CustomerEmail.Contains(term)) ||
                (o.CustomerPhoneNumber != null && o.CustomerPhoneNumber.Contains(term))
            );
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed))
        {
            q = q.Where(o => o.OrderStatus == parsed);
        }

        if (from.HasValue) q = q.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(o => o.CreatedAt <= to.Value);

        q = q.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id);

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new AdminOrderListItemDto(
                Id: o.Id,
                OrderNumber: o.OrderNumber,
                CreatedAtUtc: o.CreatedAt,
                CustomerName: ((o.CustomerFirstName ?? "") + " " + (o.CustomerLastName ?? "")).Trim(),
                CustomerEmail: o.CustomerEmail ?? string.Empty,
                OrderStatus: o.OrderStatus,
                PaymentStatus: o.Payment.Status,
                GrandTotal: o.GrandTotal,
                PaymentMethod: o.Payment.PaymentMethodType ?? string.Empty
            ))
            .ToListAsync(ct);

        return new PagedResult<AdminOrderListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize),
            Items = items
        };
    }

    public async Task<AdminOrderDetailsDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var o = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (o is null) return null;

        var items = o.OrderItems
            .OrderBy(i => i.Id)
            .Select(i => new AdminOrderItemDto(
                ProductId: i.ProductId,
                Sku: i.Sku,
                ProductName: i.ProductName,
                UnitPrice: i.UnitPrice,
                VatRate: i.VatRate,
                Quantity: i.Quantity,
                LineTotal: i.LineTotal))
            .ToList();

        var street = o.ShippingAddress?.Street ?? string.Empty;
        var postal = o.ShippingAddress?.PostalCode ?? string.Empty;
        var city = o.ShippingAddress?.City ?? string.Empty;
        var country = o.ShippingAddress?.Country ?? string.Empty;

        return new AdminOrderDetailsDto(
            Id: o.Id,
            OrderNumber: o.OrderNumber,
            CreatedAtUtc: o.CreatedAt,
            CustomerFirstName: o.CustomerFirstName ?? string.Empty,
            CustomerLastName: o.CustomerLastName ?? string.Empty,
            CustomerEmail: o.CustomerEmail ?? string.Empty,
            CustomerPhoneNumber: o.CustomerPhoneNumber ?? string.Empty,
            ShippingStreet: street,
            ShippingPostalCode: postal,
            ShippingCity: city,
            ShippingCountry: country,
            OrderStatus: o.OrderStatus,
            PaymentStatus: o.Payment.Status,
            PaymentMethod: o.Payment.PaymentMethodType ?? string.Empty,
            PaymentIntentId: o.Payment.PaymentIntentId,
            Currency: o.Currency,
            ProductsSubtotal: o.ProductsSubtotal,
            ShippingCost: o.ShippingCost,
            TaxTotal: o.TaxTotal,
            GrandTotal: o.GrandTotal,
            Items: items
        );
    }

    public async Task<bool> UpdateStatusAsync(int id, string newStatus, CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(newStatus, ignoreCase: true, out var next))
            return false;

        var o = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return false;

        switch (next)
        {
            case OrderStatus.Processing: o.MarkProcessing(); break;
            case OrderStatus.Shipped: o.MarkShipped(); break;
            case OrderStatus.Completed: o.MarkCompleted(); break;
            case OrderStatus.Cancelled: o.MarkCancelled(); break;
            case OrderStatus.Confirmed: o.MarkConfirmed(); break;
            case OrderStatus.Failed: o.MarkFailed(); break;
            case OrderStatus.Refunded: o.MarkRefunded(); break;

            case OrderStatus.Pending:
                return false;

            default: return false;
        }

        await dbContext.SaveChangesAsync(ct);
        return true;
    }
}
