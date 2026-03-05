using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seed;

public static class ProductSeeder
{
    private const int TargetCount = 250;

    public static async Task SeedAsync(PallshoppenDbContext db, CancellationToken ct = default)
    {


        var existingBySku = await db.Products
            .AsTracking()
            .Where(p => p.Sku != null)
            .ToDictionaryAsync(p => p.Sku!, ct);

        var curated = GetCurated();
        UpsertRange(db, existingBySku, curated);

        var currentCount = await db.Products.CountAsync(ct);
        var missing = Math.Max(0, TargetCount - currentCount);

        if (missing > 0)
        {
            var generated = Generate(missing, seedOffset: curated.Count);
            UpsertRange(db, existingBySku, generated);
        }

        await db.SaveChangesAsync(ct);
    }
    private static void UpsertRange(PallshoppenDbContext db, Dictionary<string, Product> existingBySku, IEnumerable<Product> incoming)
    {
        foreach (var p in incoming)
        {
            if (string.IsNullOrWhiteSpace(p.Sku))
                throw new InvalidOperationException("Seeder kräver SKU för upsert.");

            if (existingBySku.TryGetValue(p.Sku!, out var existing))
            {
                existing.Name = p.Name;
                existing.Description = p.Description;
                existing.PriceExVat = p.PriceExVat;
                existing.VatRate = p.VatRate;
                existing.Condition = p.Condition;
                existing.PalletType = p.PalletType;
                existing.OnHand = p.OnHand;
                existing.Reserved = p.Reserved;
                existing.LowStockThreshold = p.LowStockThreshold;
                existing.IsActive = p.IsActive;
                existing.Slug = p.Slug;

                UpsertImages(existing, p);
            }
            else
            {
                NormalizeImages(p);
                db.Products.Add(p);              
                existingBySku[p.Sku!] = p;
            }
        }
    }
    private static void UpsertImages(Product existing, Product incoming)
    {
        var incomingImages = incoming.Images?.ToList() ?? new List<ProductImage>();

        if (incomingImages.Count == 0)
        {
            NormalizeImages(existing);
            return;
        }

        if (existing.Images == null || existing.Images.Count == 0)
        {
            existing.Images = incomingImages;
            NormalizeImages(existing);
            return;
        }

        var incomingPrimary = incomingImages
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .FirstOrDefault();

        if (incomingPrimary is null)
        {
            NormalizeImages(existing);
            return;
        }

        var existingPrimary = existing.Images
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .FirstOrDefault();

        if (existingPrimary is null)
        {
            existing.Images.Add(new ProductImage
            {
                Url = incomingPrimary.Url,
                SortOrder = 0,
                IsPrimary = true,
                AltText = incomingPrimary.AltText
            });
        }
        else
        {
            existingPrimary.Url = incomingPrimary.Url;
            existingPrimary.AltText = incomingPrimary.AltText;
            existingPrimary.IsPrimary = true;
        }

        NormalizeImages(existing);
    }
    private static void NormalizeImages(Product p)
    {
        if (p.Images == null) p.Images = new List<ProductImage>();

        var list = p.Images
            .Where(i => !string.IsNullOrWhiteSpace(i.Url))
            .ToList();

        for (var i = 0; i < list.Count; i++)
            list[i].SortOrder = i;

        if (list.Count > 0)
        {
            var firstPrimary = list.FirstOrDefault(i => i.IsPrimary) ?? list[0];
            foreach (var img in list) img.IsPrimary = ReferenceEquals(img, firstPrimary);
            firstPrimary.IsPrimary = true;
        }
        p.Images = list;
    }
    private static List<Product> GetCurated()
    {

        return new()
        {
            Make(
                sku: "PAL-EUR-N-001",
                name: "EURO-pall Ny, premium",
                type: ProductType.EuroPallet,
                condition: ProductCondition.New,
                price: 169m,
                onHand: 500,
                reserved: 10,
                img: "https://picsum.photos/seed/pallet-eur-1/800/600",
                desc: "Ny EUR-pall, perfekt för logistik och lager. Stabil, standardmått, inga överraskningar."
            ),
            Make(
                sku: "PAL-EUR-U-001",
                name: "EURO-pall Begagnad, klass B",
                type: ProductType.EuroPallet,
                condition: ProductCondition.Used,
                price: 89m,
                onHand: 80,
                reserved: 30,
                img: "https://picsum.photos/seed/pallet-eur-2/800/600",
                desc: "Begagnad EUR-pall. Lite charm, lite skav, fungerar fortfarande. Som de flesta människor."
            ),
            Make(
                sku: "PAL-IND-R-001",
                name: "Industri-pall Upprustad, heavy duty",
                type: ProductType.IndustrialPallet,
                condition: ProductCondition.Refurbished,
                price: 249m,
                onHand: 25,
                reserved: 24,
                img: "https://picsum.photos/seed/pallet-ind-1/800/600",
                desc: "Upprustad industripall med extra bärighet. Nästan slut i lager (för att testa low-stock)."
            ),
            Make(
                sku: "PAL-HALF-N-001",
                name: "Halv-pall Ny",
                type: ProductType.HalfPallet,
                condition: ProductCondition.New,
                price: 119m,
                onHand: 0,
                reserved: 0,
                img: "https://picsum.photos/seed/pallet-half-1/800/600",
                desc: "Ny halvpall. Slut i lager (för att testa out-of-stock)."
            ),
            Make(
                sku: "PAL-CUST-N-001",
                name: "Specialmåttad pall (Custom)",
                type: ProductType.CustomPallet,
                condition: ProductCondition.New,
                price: 399m,
                onHand: 12,
                reserved: 0,
                img: "https://picsum.photos/seed/pallet-custom-1/800/600",
                desc: "Måttad efter behov. Bra när standard inte räcker och man vägrar kompromissa."
            ),
            Make(
                sku: "PAL-OTH-U-001",
                name: "Övrigt: Pallkrage kit (begagnad)",
                type: ProductType.Other,
                condition: ProductCondition.Used,
                price: 59m,
                onHand: 200,
                reserved: 0,
                img: "https://picsum.photos/seed/pallet-other-1/800/600",
                desc: "Pallkragar och blandat. Bra för lager, odling, eller att bygga små fästningar av trä."
            ),
        };
    }
    private static IEnumerable<Product> Generate(int count, int seedOffset)
    {
        var rnd = new Random(1337 + seedOffset);

        var types = Enum.GetValues<ProductType>();
        var conds = Enum.GetValues<ProductCondition>();

        var adjectives = new[] { "Premium", "Standard", "Budget", "Heavy Duty", "Lättvikt", "Staplingsbar", "Slagtålig" };
        var tags = new[] { "HT", "EPAL", "ISPM15", "Industriklass", "EU-standard", "Lager", "Transport" };

        for (var i = 1; i <= count; i++)
        {
            var type = types[rnd.Next(types.Length)];
            var condition = conds[rnd.Next(conds.Length)];

            var basePrice = type switch
            {
                ProductType.EuroPallet => 110m,
                ProductType.HalfPallet => 85m,
                ProductType.IndustrialPallet => 190m,
                ProductType.CustomPallet => 320m,
                ProductType.SpecialPallet => 210m,
                _ => 70m
            };

            var conditionFactor = condition switch
            {
                ProductCondition.New => 1.25m,
                ProductCondition.Refurbished => 1.05m,
                _ => 0.80m
            };

            var jitter = (decimal)rnd.Next(-15, 35); 
            var price = Math.Max(19m, basePrice * conditionFactor + jitter);

            var mode = rnd.Next(100);
            int onHand, reserved;

            if (mode < 10)
            {
                onHand = 0;
                reserved = 0;
            }
            else if (mode < 30)
            {
                onHand = rnd.Next(5, 25);
                reserved = rnd.Next(0, Math.Min(onHand, 20));
            }
            else
            {
                onHand = rnd.Next(30, 800);
                reserved = rnd.Next(0, Math.Min(onHand, 120));
            }

            var adj = adjectives[rnd.Next(adjectives.Length)];
            var tag = tags[rnd.Next(tags.Length)];

            var name = $"{TypeLabel(type)} {adj} ({tag})";
            var sku = SkuFor(type, condition, i + seedOffset);

            var desc =
                $"{adj} {TypeLabel(type).ToLowerInvariant()} i skick: {ConditionLabel(condition).ToLowerInvariant()}. " +
                $"Märkt för {tag}. Passar för lager/transport och andra mänskliga projekt som går ut på att flytta saker från A till B.";

            yield return Make(
                sku: sku,
                name: name,
                type: type,
                condition: condition,
                price: decimal.Round(price, 0),
                onHand: onHand,
                reserved: reserved,
                img: $"https://picsum.photos/seed/{Slugify(sku)}/800/600",
                desc: desc
            );
        }
    }
    private static Product Make(
        string sku,
        string name,
        ProductType type,
        ProductCondition condition,
        decimal price,
        int onHand,
        int reserved,
        string img,
        string desc)
    {
        return new Product
        {
            Name = name,
            Description = desc,
            PriceExVat = price,
            VatRate = VatRate.Vat25,
            PalletType = type,
            Condition = condition,
            OnHand = onHand,
            Reserved = reserved,
            LowStockThreshold = 20,
            IsActive = true,
            Sku = sku,
            Slug = Slugify($"{name}-{sku}"),

            Images = new List<ProductImage>
            {
                new ProductImage
                {
                    Url = img,
                    SortOrder = 0,
                    IsPrimary = true,
                    AltText = name
                }
            }
        };
    }
    private static string SkuFor(ProductType type, ProductCondition condition, int n)
    {
        var t = type switch
        {
            ProductType.EuroPallet => "EUR",
            ProductType.HalfPallet => "HALF",
            ProductType.IndustrialPallet => "IND",
            ProductType.CustomPallet => "CUST",
            ProductType.SpecialPallet => "SPEC",
            _ => "OTH"
        };

        var c = condition switch
        {
            ProductCondition.New => "N",
            ProductCondition.Refurbished => "R",
            _ => "U"
        };

        return $"PAL-{t}-{c}-{n:0000}";
    }
    private static string TypeLabel(ProductType t) => t switch
    {
        ProductType.EuroPallet => "EURO-pall",
        ProductType.HalfPallet => "Halv-pall",
        ProductType.IndustrialPallet => "Industri-pall",
        ProductType.CustomPallet => "Specialmåttad pall",
        ProductType.SpecialPallet => "Special-pall",
        _ => "Övrigt"
    };
    private static string ConditionLabel(ProductCondition c) => c switch
    {
        ProductCondition.New => "Ny",
        ProductCondition.Refurbished => "Upprustad",
        _ => "Begagnad"
    };
    private static string Slugify(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Trim().ToLowerInvariant();

        var chars = s
            .Replace("å", "a").Replace("ä", "a").Replace("ö", "o")
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();

        var slug = new string(chars);
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}
