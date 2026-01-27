using Application.DTOs.Admin;
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Domain.Factories;
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
            throw new InvalidOperationException("Cart ID is required to create an order.");

        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new InvalidOperationException("Order number is required.");

        var currency = string.IsNullOrWhiteSpace(dto.Currency)
            ? "SEK"
            : dto.Currency.Trim().ToUpperInvariant();

        var byProduct = dto.Items
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Qty = g.Sum(x => x.Quantity)
            })
            .ToList();

        if (byProduct.Any(x => x.ProductId <= 0))
            throw new InvalidOperationException("Product Id must be valid.");

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
            throw new InvalidOperationException("One or more products are invalid or inactive.");

        var order = new Order(orderNumber, currency);
        order.SetCartId(dto.CartId);

        var items = new List<OrderItem>(byProduct.Count);

        foreach (var x in byProduct)
        {
            var p = products.First(pp => pp.Id == x.ProductId);

            if (string.IsNullOrWhiteSpace(p.Name))
                throw new InvalidOperationException($"Product name is missing for productId={p.Id}.");

            if (p.PriceExVat < 0)
                throw new InvalidOperationException($"PriceExVat is invalid for productId={p.Id}.");

            var vatPercent = (int)p.VatRate;
            if (vatPercent is not (6 or 12 or 25))
                throw new InvalidOperationException($"VatRate is invalid for productId={p.Id}.");

            var productForFactory = new Product
            {
                Id = p.Id,
                Name = p.Name.Trim(),
                Sku = string.IsNullOrWhiteSpace(p.Sku) ? null : p.Sku.Trim(),
                PriceExVat = p.PriceExVat,
                VatRate = p.VatRate
            };

            items.Add(OrderItemFactory.FromProductExVat(productForFactory, x.Qty));
        }

        order.ReplaceItems(items);

        order.SetShippingCost(0m);

        return order;
    }

    public OrderDetailsDto ToDetailsDto(Order o) =>
        new(
            o.Id,
            o.OrderNumber,
            o.CreatedAt,
            o.Currency,
            o.ProductsSubtotal,
            o.ShippingCost,
            o.VatTotal,
            o.GrandTotal,
            o.OrderStatus,
            o.Payment.Status,

            o.CustomerFirstName,
            o.CustomerLastName,
            o.CustomerEmail,
            o.CustomerPhoneNumber,

            o.ShippingAddress is null ? null : new ShippingAddressDto(
                o.ShippingAddress.Street,
                o.ShippingAddress.City,
                o.ShippingAddress.PostalCode,
                o.ShippingAddress.Country
            ),
            o.ShippingCarrier,
            o.ShippingMethod,
            o.ServicePointId,

            o.TrackingNumber,
            o.TrackingNumber is null
                ? null
                : $"https://tracking.postnord.com/?id={Uri.EscapeDataString(o.TrackingNumber)}",

            o.OrderItems
                .Select(i => new OrderItemDto(
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPriceExVat,
                    i.LineTotalExVat

                ))
                .ToList(),

            o.UserId
        );



    public OrderCreatedDto ToCreatedDto(Order o) =>
    new(
        o.Id,
        o.OrderNumber,
        o.CreatedAt,
        o.Currency,
        o.ProductsSubtotal,
        o.ShippingCost,
        o.VatTotal,
        o.GrandTotal,
        o.OrderStatus,
        o.Payment.Status,

        o.CustomerFirstName,
        o.CustomerLastName,
        o.CustomerEmail,
        o.CustomerPhoneNumber,

        o.ShippingAddress is null
            ? null
            : new ShippingAddressDto(
                o.ShippingAddress.Street,
                o.ShippingAddress.City,
                o.ShippingAddress.PostalCode,
                o.ShippingAddress.Country
            ),
        o.ShippingCarrier,
        o.ShippingMethod,
        o.ServicePointId,

        o.UserId
    );
}
