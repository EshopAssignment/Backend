using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Application.Assemblers;

public sealed class OrderAssembler(IAppDbContext dbContext)
{
    private readonly IAppDbContext _db = dbContext;

    public async Task<Order> FromDtoAsync(CreateOrderRequestDto dto, string orderNumber, CancellationToken ct)
    {
        if (dto is null || dto.Items.Count == 0)
            throw new InvalidOperationException("Order DTO is null or contains no items.");

        if (string.IsNullOrWhiteSpace(dto.CartId))
            throw new Exception("Cart ID is required to create an order.");


        var currency = string.IsNullOrWhiteSpace(dto.Currency) ? "SEK" : dto.Currency.Trim().ToUpperInvariant();

        var byProduct = dto.Items
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Qty = g.Sum(x => x.Quantity)
            }).ToList();

        if (byProduct.Any(x => x.ProductId <= 0))
            throw new InvalidOperationException("Product Id must be valid");

        if (byProduct.Any(x => x.Qty <= 0))
            throw new InvalidOperationException("Quantity must be >= 1.");

        var ids = byProduct.Select(x => x.ProductId).Distinct().ToList();

        var products = await _db.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id) && p.IsActive)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Sku,
                p.PriceExVat,
                p.VatRate
            })
            .ToListAsync(ct);

        if (products.Count != ids.Count)
            throw new InvalidOperationException("One or more Products are invalid or Inactive");

        var order = new Order(orderNumber, currency);
        order.SetCartId(dto.CartId);

        var items = new List<OrderItem>(byProduct.Count);

        foreach(var x in byProduct)
        {
            var p = products.First(pp => pp.Id == x.ProductId);

            if (string.IsNullOrWhiteSpace(p.Name))
                throw new InvalidOperationException($"Product Name is missing productId={p.Id}.");

            if (p.PriceExVat < 0)
                throw new InvalidOperationException($"Price is Invalid for productId={p.Id}");

            if (p.VatRate < 0)
                throw new InvalidOperationException($"VatRate is productId={p.Id}.");

            var lineTotal = p.PriceExVat * x.Qty;

            items.Add(new OrderItem
            {
                ProductId = p.Id,
                Sku = string.IsNullOrWhiteSpace(p.Sku) ? $"PID-{p.Id}" : p.Sku.Trim(),
                ProductName = p.Name.Trim(),
                UnitPrice = p.PriceExVat,
                VatRate = p.VatRate,
                Quantity = x.Qty,
                LineTotal = lineTotal,
            });
        }

        order.ReplaceItems(items);

        order.SetShippingCost(0m);

        var taxTotal = items.Sum(i => i.LineTotal * i.VatRate);
        order.SetTaxTotal(taxTotal);

        return order;
    }
}
