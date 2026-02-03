using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seed;

public static class ProductSeeder
{
    public static async Task SeedAsync(PallshoppenDbContext db, CancellationToken ct)
    {
        const int targetCount = 60;

        var existing = await db.Products.CountAsync(ct);
        if (existing >= targetCount)
            return;

        var rng = new Random(42); 
        var products = new List<Product>();

        while (existing + products.Count < targetCount)
        {
            foreach (var tpl in ProductSeedData.Templates)
            {
                if (existing + products.Count >= targetCount)
                    break;

                var condition = RandomEnum<ProductCondition>(rng);
                var vat = RandomEnum<VatRate>(rng);

                var onHand = rng.Next(0, 400);
                var reserved = rng.Next(0, Math.Min(onHand, 50));

                var variant = rng.Next(1000, 9999);
                var name = $"{tpl.BaseName} {condition} #{variant}";

                products.Add(new Product
                {
                    Name = name,
                    Description =
                        $"{tpl.BaseName} i skick {condition}. Lämplig för lager, transport och e-handel.",
                    ImgUrl = "/images/products/pallet-placeholder.jpg",

                    PriceExVat = rng.Next(80, 300),
                    VatRate = vat,
                    Condition = condition,
                    PalletType = tpl.Type,

                    OnHand = onHand,
                    Reserved = reserved,
                    LowStockThreshold = rng.Next(10, 40),

                    IsActive = rng.NextDouble() > 0.05,

                    Sku = $"PAL-{tpl.Type}-{variant}",
                    Slug = Slugify(name)
                });
            }
        }

        db.Products.AddRange(products);
        await db.SaveChangesAsync(ct);
    }

    private static T RandomEnum<T>(Random rng) where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(rng.Next(values.Length))!;
    }

    private static string Slugify(string value)
        => value
            .ToLowerInvariant()
            .Replace("å", "a")
            .Replace("ä", "a")
            .Replace("ö", "o")
            .Replace(" ", "-");
}
