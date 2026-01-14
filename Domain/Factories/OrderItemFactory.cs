using Domain.Entities;

namespace Domain.Factories;

public static class OrderItemFactory
{
    public static OrderItem CreateExVat(int productId, string sku, string productName, decimal unitPriceExVat, decimal vatRate, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPriceExVat < 0) throw new ArgumentOutOfRangeException(nameof(unitPriceExVat));
        if (vatRate < 0) throw new ArgumentOutOfRangeException(nameof(vatRate));

        return new OrderItem
        {
            ProductId = productId,
            Sku = sku,
            ProductName = productName,
            UnitPrice = unitPriceExVat,
            VatRate = vatRate,
            Quantity = quantity,
            LineTotal = Math.Round(unitPriceExVat * quantity, 2, MidpointRounding.AwayFromZero)
        };
    }

    public static OrderItem FromProductExVat(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        var sku = string.IsNullOrWhiteSpace(product.Sku) ? $"PID-{product.Id}" : product.Sku.Trim();

        return CreateExVat(
            productId: product.Id,
            sku: sku,
            productName: product.Name,
            unitPriceExVat: product.PriceExVat,
            vatRate: product.VatRate, 
            quantity: quantity
        );
    }
}
