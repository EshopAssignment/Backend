
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class OrderService(PallshoppenDbContext dbContext) : IOrderService
{
    public async Task<OrderCreatedDto> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new InvalidOperationException("Order must contain one item");
        }


        var lines = request.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        if (lines.Any(l => l.Quantity <= 0))
            throw new ArgumentOutOfRangeException(nameof(request.Items), "Quantity must be above 0");

        var productIds = lines.Select(l => l.ProductId).ToList();


        var products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
            throw new InvalidOperationException("One or more products a invalid.");

        foreach (var l in lines)
        {
            var p = products.First(x => x.Id == l.ProductId);
            var available = Math.Max(0, p.OnHand - p.Reserved);
            if (l.Quantity > available)
                throw new InvalidOperationException($"Insufficient stock for product {p.Name} (id {p.Id}).");
        }

        var order = new Order
        {
            OrderNumber = await GenerateUniqueOrderNumberAsync(cancellationToken),
            OrderDate = DateTime.UtcNow,
            CustomerFirstName = request.CustomerFirstName,
            CustomerLastName = request.CustomerLastName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhoneNumber = request.CustomerPhoneNumber,
            ShippingStreet = request.ShippingStreet,
            ShippingCity = request.ShippingCity,
            ShippingPostalCode = request.ShippingPostalCode,
            ShippingCountry = request.ShippingCountry,
            OrderStatus = "New"
        };


        decimal productsTotal = 0m;
        var orderItems = new List<OrderItem>(lines.Count);

        foreach (var l in lines)
        {
            var p = products.First(x => x.Id == l.ProductId);

            var unitPrice = p.PriceExVat;
            var lineTotal = Math.Round(unitPrice * l.Quantity, 2, MidpointRounding.AwayFromZero);

            orderItems.Add(new OrderItem
            {
                ProductId = p.Id,
                ProductName = p.Name,
                UnitPrice = unitPrice,
                Quantity = l.Quantity,
                LineTotal = lineTotal,
                Order = order
            });
            productsTotal += lineTotal;
        }

        order.ProductsTotal = Math.Round(productsTotal, 2, MidpointRounding.AwayFromZero);
        order.ShippingCost = 0m;
        order.Total = Math.Round(order.ProductsTotal + order.ShippingCost, 2, MidpointRounding.AwayFromZero);
        order.OrderItems = orderItems;

        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var g in lines)
        {
            var affected = await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE Products SET OnHand = OnHand - {0} WHERE Id = {1} AND (OnHand - Reserved) >= {0}",
                [g.Quantity, g.ProductId], cancellationToken);

            if (affected == 0)
            {
                await tx.RollbackAsync(cancellationToken);
                throw new InvalidOperationException($"Insufficient stock for product id {g.ProductId}.");
            }
        }
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return new OrderCreatedDto(
            order.Id,
            order.OrderNumber,
            order.OrderDate,
            order.Total
        );
    }
    public async Task<OrderCreatedDto?> GetOrderByIdAsync(int id, CancellationToken ct)
    {
        var order = await dbContext.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null) return null;

        return new OrderCreatedDto(order.Id, order.OrderNumber, order.OrderDate, order.Total);
    }

    //Tiny helpers to generate a ordernumber. 
    private async Task<string> GenerateUniqueOrderNumberAsync(CancellationToken ct)
    {
        for (var i = 0; i < 3; i++)
        {
            var candidate = GenerateOrderNumber();
            var exists = await dbContext.Orders.AsNoTracking()
                .AnyAsync(o => o.OrderNumber == candidate, ct);
            if (!exists) return candidate;
        }
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}".ToUpperInvariant();
    }
    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var randomSuffix = Random.Shared.Next(1000, 9999);
        return $"ORD-{timestamp}-{randomSuffix}";
    }
}
