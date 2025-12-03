using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(PallshoppenDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(cancellationToken))
            return;

        string Slugify(string input) =>
            input.Trim().ToLower().Replace(" ", "-").Replace("(", "").Replace(")", "");

        string SkuOf(string name) =>
            "SKU-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

        var products = new List<Product>
        {
            new()
            {
                Name = "EU-Pall",
                Description = "Den Perfekta pallen",
                PalletType = ProductType.EuroPallet,
                Condition = ProductCondition.New,
                PriceExVat = 300,
                OnHand = 100,
                Reserved = 0,
                ImgUrl = "/images/not-implemented.jpg",
                IsActive = true,
                Slug = Slugify("EU-Pall"),
                Sku = SkuOf("EU-Pall")
            },
            new()
            {
                Name = "Pallkrage",
                Description = "Den perfekta Pallkragen",
                PalletType = ProductType.SpecialPallet,
                Condition = ProductCondition.New,
                PriceExVat = 230,
                OnHand = 300,
                Reserved = 0,
                ImgUrl = "/images/not-implemented.jpg",
                IsActive = true,
                Slug = Slugify("Pallkrage"),
                Sku = SkuOf("Pallkrage")
            },
            new()
            {
                Name = "Halvpall",
                Description = "Storleken spelar ingen roll",
                PalletType = ProductType.HalfPallet,
                Condition = ProductCondition.Refurbished,
                PriceExVat = 150,
                OnHand = 200,
                Reserved = 0,
                ImgUrl = "/images/not-implemented.jpg",
                IsActive = true,
                Slug = Slugify("Halvpall"),
                Sku = SkuOf("Halvpall")
            },
            new()
            {
                Name = "Spån",
                Description = "Spån utvunnen från spillvirke",
                PalletType = ProductType.Other,
                Condition = ProductCondition.New,
                PriceExVat = 50,
                OnHand = 200,
                Reserved = 0,
                ImgUrl = "/images/not-implemented.jpg",
                IsActive = true,
                Slug = Slugify("Spån"),
                Sku = SkuOf("Spån")
            },
            new()
            {
                Name = "Kontainer Pall",
                Description = "XXL pallmodell",
                PalletType = ProductType.IndustrialPallet,
                Condition = ProductCondition.New,
                PriceExVat = 500,
                OnHand = 200,
                Reserved = 0,
                ImgUrl = "/images/not-implemented.jpg",
                IsActive = true,
                Slug = Slugify("Kontainer Pall"),
                Sku = SkuOf("Kontainer Pall")
            },
            new()
            {
                Name = "EU-Pall (Refurbished)",
                Description = "Den perfekta pallen, nu lagad",
                PalletType = ProductType.EuroPallet,
                Condition = ProductCondition.Refurbished,
                PriceExVat = 120,
                OnHand = 50,
                Reserved = 0,
                ImgUrl = "/images/not-implemented.jpg",
                IsActive = true,
                Slug = Slugify("EU-Pall Refurbished"),
                Sku = SkuOf("EU-Pall Refurbished")
            }
        };

        await db.Products.AddRangeAsync(products, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
