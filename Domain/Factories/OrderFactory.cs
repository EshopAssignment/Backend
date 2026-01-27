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

        var order = new Order(orderNumber, currency);
        order.SetShippingAddress(shipping);

        var items = lines.Select(x => OrderItemFactory.FromProductExVat(x.Product, x.Quantity)).ToList();
        order.ReplaceItems(items);

        if (shippingCost < 0) throw new ArgumentOutOfRangeException(nameof(shippingCost));
        order.SetShippingCost(shippingCost);

        return order;
    }

    public static Order CreateFromItems(string orderNumber, ShippingAddress shipping, IEnumerable<OrderItem> items, decimal shippingCost = 0m, string currency = "SEK")
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number required", nameof(orderNumber));

        ArgumentNullException.ThrowIfNull(shipping);
        ArgumentNullException.ThrowIfNull(items);

        var order = new Order(orderNumber, currency);
        order.SetShippingAddress(shipping);

        order.ReplaceItems(items);

        if (shippingCost < 0) throw new ArgumentOutOfRangeException(nameof(shippingCost));
        order.SetShippingCost(shippingCost);

        return order;
    }
}
