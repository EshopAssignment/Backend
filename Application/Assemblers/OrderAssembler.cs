using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Domain.Factories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Application.Assemblers;

public sealed class OrderAssembler(IAppDbContext dbContext)
{
    private readonly IAppDbContext _db = dbContext;

    public async Task<Order> FromDtoAsync(CreateOrderRequestDto dto, string orderNumber, CancellationToken ct)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            throw new InvalidOperationException("Order must contain at least one item.");

        // trimma och validera kundfält (så vi inte råkar få null/space)
        var first = (dto.CustomerFirstName ?? "").Trim();
        var last = (dto.CustomerLastName ?? "").Trim();
        var email = (dto.CustomerEmail ?? "").Trim();
        var phone = (dto.CustomerPhoneNumber ?? "").Trim();

        if (first.Length == 0) throw new ArgumentException("CustomerFirstName required");
        if (last.Length == 0) throw new ArgumentException("CustomerLastName required");
        if (email.Length == 0) throw new ArgumentException("CustomerEmail required");
        if (phone.Length == 0) throw new ArgumentException("CustomerPhoneNumber required");

        var street = (dto.ShippingAddress.Street ?? "").Trim();
        var zip = (dto.ShippingAddress.PostalCode ?? "").Trim();
        var city = (dto.ShippingAddress.City ?? "").Trim();
        var country = (dto.ShippingAddress.Country ?? "SE").Trim();

        if (street.Length == 0) throw new ArgumentException("ShippingStreet required");
        if (zip.Length == 0) throw new ArgumentException("ShippingPostalCode required");
        if (city.Length == 0) throw new ArgumentException("ShippingCity required");

        var shipping = new ShippingAddress(street, zip, city, country);

        var byProduct = dto.Items
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToList();

        if (byProduct.Any(x => x.Qty <= 0))
            throw new ArgumentOutOfRangeException(nameof(dto.Items), "Quantity must be > 0");

        var ids = byProduct.Select(x => x.ProductId).ToList();

        var products = await dbContext.Products
            .Where(p => ids.Contains(p.Id) && p.IsActive)
            .ToListAsync(ct);

        if (products.Count != ids.Count)
            throw new InvalidOperationException("One or more products are invalid or inactive.");

        var lines = byProduct
            .Select(x =>
            {
                var p = products.First(pp => pp.Id == x.ProductId);
                return (Product: p, Quantity: x.Qty);
            })
            .ToList();

        var order = OrderFactory.Create(
            orderNumber: orderNumber,
            shipping: shipping,
            lines: lines,
            shippingCost: 0m,
            currency: "SEK"
        );

        order.CustomerFirstName = first;
        order.CustomerLastName = last;
        order.CustomerEmail = email;
        order.CustomerPhoneNumber = phone;

        return order;
    }
}
