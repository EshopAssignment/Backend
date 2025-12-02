
using Application.DTOs.Admin;
using Application.DTOs.Product;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AdminOrderService(PallshoppenDbContext dbContext) : IAdminOrderService
{
    public async Task<PagedResult<AdminOrderListItemDto>> GetAllAsync(int page, int pageSize, string? query, string? status, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = dbContext.Orders.AsQueryable();

        if(!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(o =>
            o.OrderNumber.Contains(term) ||
            o.CustomerFirstName.Contains(term) ||
            o.CustomerLastName.Contains(term) ||
            o.CustomerEmail.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            q = q.Where(o =>
            o.OrderStatus == status);
        }

        if (from.HasValue) q = q.Where(o => o.OrderDate >= from.Value);
        if (to.HasValue) q = q.Where(o => o.OrderDate  <= to.Value);

        q = q.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id);

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((page -1) * pageSize).Take(pageSize)
            .Select(o => new AdminOrderListItemDto(
                o.Id,
                o.OrderNumber,
                o.CustomerFirstName + " " + o.CustomerLastName,
                o.CustomerEmail,
                o.OrderStatus,
                o.Total
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
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (o is null) return null;

        var items = o.OrderItems
            .OrderBy(i => i.Id)
            .Select(i => new AdminOrderItemDto(
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.LineTotal))
            .ToList();

        return new AdminOrderDetailsDto(
                o.Id,
                o.OrderNumber,
                o.OrderDate,
                o.CustomerFirstName,
                o.CustomerLastName,
                o.CustomerEmail,
                o.CustomerPhoneNumber,
                o.ShippingStreet,
                o.ShippingPostalCode,
                o.ShippingCity,
                o.ShippingCountry,
                o.OrderStatus,
                o.ProductsTotal,
                o.ShippingCost,
                o.Total,
                items
            );
    }

    public async Task<bool> UpdateStatusAsync(int id, string newStatus, CancellationToken ct)
    {
        var o = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return false;

        o.OrderStatus = newStatus;
        await dbContext.SaveChangesAsync(ct);
        return true;
    } 
}
