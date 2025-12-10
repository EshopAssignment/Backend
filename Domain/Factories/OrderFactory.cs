using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Factories;

public static class OrderFactory
{
    public static Order Create(string orderNumber,ShippingAddress shipping,IEnumerable<(Product Product, int Quantity)> lines,decimal shippingCost = 0m,string currency = "SEK")
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number required", nameof(orderNumber));

        ArgumentNullException.ThrowIfNull(shipping);
        ArgumentNullException.ThrowIfNull(lines);

        var order = new Order(orderNumber, shipping, currency);

        var items = new List<OrderItem>();
        foreach (var (product, qty) in lines)
            items.Add(OrderItemFactory.FromProduct(product, qty));

        order.ReplaceItems(items);
        if(shippingCost > 0) order.SetShippingCost(shippingCost);
        order.SetTaxTotal(0);

        return order;
    }

    public static Order CreateFromItems(string orderNumber, ShippingAddress shipping, IEnumerable<OrderItem> items, decimal shippingCost = 0m, string currency = "SEK")
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number required", nameof(orderNumber));

        ArgumentNullException.ThrowIfNull(shipping);
        ArgumentNullException.ThrowIfNull(items);

        var order = new Order(orderNumber, shipping, currency);
        order.ReplaceItems(items);
        if (shippingCost > 0) order.SetShippingCost(shippingCost);
        order.SetTaxTotal(0);
        return order;
    }
}
