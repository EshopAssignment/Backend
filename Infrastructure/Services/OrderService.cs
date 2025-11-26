
using System.Linq;
using Application.DTOs;
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

        var productIds = request.Items.Select(i => i.ProductId).ToList();

        var products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync(cancellationToken);



        if (products.Count != productIds.Count)
        {
            throw new InvalidOperationException("One or more products are invalid, inactive or missing.");
        }

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
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

        var orderItems = new List<OrderItem>();
        decimal productsTotal = 0m;

        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            var unitPrice = product.Price;
            var lineTotal = unitPrice * item.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = unitPrice,
                Quantity = item.Quantity,
                LineTotal = lineTotal,
                Order = order,

            };

            productsTotal += lineTotal;
            orderItems.Add(orderItem);

        }

        order.ProductsTotal = productsTotal;

        //placeholder Shippingcost before the actual shippingparts are implemented. 
        order.ShippingCost = 0m;
        order.Total = order.ProductsTotal + order.ShippingCost;

        order.OrderItems = orderItems;

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

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

        return new OrderCreatedDto(
            order.Id,
            order.OrderNumber,
            order.OrderDate,
            order.Total
        );
    }




    //Tiny helper to generate a ordernumber. 
    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var randomSuffix = Random.Shared.Next(1000, 9999);
        return $"ORD-{timestamp}-{randomSuffix}";
    }
}
