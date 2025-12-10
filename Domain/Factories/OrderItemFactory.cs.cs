using Domain.Entities;

namespace Domain.Factories;

public static class OrderItemFactory
{
    public static OrderItem Create(int productId, string sku, string productName, decimal unitPriceInclVat, decimal vatRate, int quantity)
    {
        return new OrderItem
        {
            ProductId = productId,
            Sku = sku,
            ProductName = productName,
            UnitPrice = unitPriceInclVat,
            VatRate = vatRate,
            Quantity = quantity,
            LineTotal = Math.Round(unitPriceInclVat * quantity, 2, MidpointRounding.AwayFromZero)
        };
    }

    public static OrderItem FromProduct(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        var unitPriceInclVat = Math.Round(product.PriceExVat * (1 + product.VatRate / 100), 2, MidpointRounding.AwayFromZero);
        return Create(
            productId: product.Id,
            sku: product.Sku ?? string.Empty,
            productName: product.Name,
            unitPriceInclVat: unitPriceInclVat,
            vatRate: product.VatRate,
            quantity: quantity
            );
    }
}
