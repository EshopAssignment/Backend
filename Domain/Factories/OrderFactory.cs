using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Factories;

public static class OrderFactory
{
    public static Order Create(string orderNumber,ShippingAddress shipping,IEnumerable<(Product Product, int Quantity)> lines,decimal shippingCost = 0m,string currency = "SEK")
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number required", nameof(orderNumber));

        ArgumentNullException.ThrowIfNull(lines);

        var order = new Order(orderNumber, currency);

        var items = new List<OrderItem>();
        foreach (var (product, qty) in lines)
            items.Add(OrderItemFactory.FromProductExVat(product, qty));

        order.ReplaceItems(items);

        order.SetShippingCost(0m);

        var tax = items.Sum(i => i.LineTotal * i.VatRate);
        order.SetTaxTotal(tax);

        return order;
    }

    public static Order CreateFromItems(string orderNumber, ShippingAddress shipping, IEnumerable<OrderItem> items, decimal shippingCost = 0m, string currency = "SEK")
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number required", nameof(orderNumber));

        ArgumentNullException.ThrowIfNull(shipping);
        ArgumentNullException.ThrowIfNull(items);

        var order = new Order(orderNumber, currency);
        order.ReplaceItems(items);
        if (shippingCost > 0) order.SetShippingCost(shippingCost);
        order.SetTaxTotal(0);
        return order;
    }
}
