
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(PallshoppenDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(cancellationToken))
        {
            return;
        }
        //Seed products. Add more if needed with the same pattern. 
        var products = new List<Product>
        {
            new()
            {
                Name = "EU-Pall",
                Description = "Den Perfekta pallen",
                PalletType = "EUR",
                Condition = "Ny",
                Price = 300,
                StockQuantity = 100,
                ImgUrl = "/images/not-implemented.jpg",
                IsActive = true
            },
            new()
            {
                Name = "PallKrage",
                Description = "Den perfekta Pallkragen",
                PalletType = "PallKrage",
                Condition = "Ny",
                Price = 230,
                StockQuantity = 300,
                ImgUrl = "/images/not-implemented.jpg",
                IsActive = true
            },
            new()
            {
                Name = "Halvpall",
                Description = "Storleken spelar ingen roll",
                PalletType = "Halvpall",
                Condition = "Refurbished",
                Price = 150,
                StockQuantity = 200,
                ImgUrl = "/images/not-implemented.jpg",
                IsActive = true
            }
        };

        await db.Products.AddRangeAsync(products, cancellationToken); // Add the products to the DbContext and consumes cancellationToken
        await db.SaveChangesAsync(cancellationToken); // Save changes to the database and consumes cancellationToken
    }
}
