using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Domain.Factories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Application.Assemblers;

public sealed class OrderAssembler(IAppDbContext db)
{
    private readonly IAppDbContext _db = db;

    public async Task<Order> FromDtoAsync(CreateOrderRequestDto dto, string orderNumber, CancellationToken ct)
    {
        var  ids = dto.Items.Select(i => i.ProductId).Distinct().ToArray();

        var product = await _db.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);

        var found = product.Select(p => p.Id).ToHashSet();
        var missing = ids.Where(id => !found.Contains(id)).ToArray();

        if(missing.Length > 0)
        {
            throw new InvalidOperationException($"Products not found: {string.Join(", ", missing)}");
        }

        var lines = dto.Items.Select(i =>
        {
            var p = product.First(x => x.Id == i.ProductId);
            return (Product: p, i.Quantity);
        }).ToList();

        var adress = new ShippingAddress(
            dto.ShippingAddress.Street,
            dto.ShippingAddress.City,
            dto.ShippingAddress.PostalCode,
            dto.ShippingAddress.Country);


        var shippingCost = dto.ShippingCost ?? 0m;
        var currency = string.IsNullOrWhiteSpace(dto.Currency) ? "SEK" : dto.Currency;


        return OrderFactory.Create(orderNumber, adress, lines, shippingCost, currency);
    }
}
