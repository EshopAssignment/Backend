using Domain.Entities;
using Domain.Enums;

namespace Domain.Factories;

public static class OrderItemFactory
{
    public static OrderItem CreateFromExVat(int productId, string sku, string productName, decimal unitPriceExVat, VatRate vatRate, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPriceExVat < 0) throw new ArgumentOutOfRangeException(nameof(unitPriceExVat));

        var vatPercent = (int)vatRate;          
        var vatMultiplier = vatPercent / 100m;  

        var unitVat = RoundMoney(unitPriceExVat * vatMultiplier);
        var unitInc = RoundMoney(unitPriceExVat + unitVat);

        var lineEx = RoundMoney(unitPriceExVat * quantity);
        var lineVat = RoundMoney(unitVat * quantity);
        var lineInc = RoundMoney(lineEx + lineVat);

        return new OrderItem
        {
            ProductId = productId,
            Sku = sku,
            ProductName = productName,
            Quantity = quantity,

            UnitPriceExVat = RoundMoney(unitPriceExVat),
            VatRatePercent = vatPercent,
            UnitVatAmount = unitVat,
            UnitPriceIncVat = unitInc,

            LineTotalExVat = lineEx,
            LineTotalVat = lineVat,
            LineTotalIncVat = lineInc
        };
    }

    public static OrderItem FromProductExVat(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        var sku = string.IsNullOrWhiteSpace(product.Sku) ? $"PID-{product.Id}" : product.Sku.Trim();

        return CreateFromExVat(
            productId: product.Id,
            sku: sku,
            productName: product.Name,
            unitPriceExVat: product.PriceExVat,
            vatRate: product.VatRate,
            quantity: quantity
            );
    }

    private static decimal RoundMoney(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
