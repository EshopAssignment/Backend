
using Infrastructure.Persistence;

namespace Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(PallshoppenDbContext db, CancellationToken cancellationToken = default)
    {
        await ProductSeeder.SeedAsync(db, cancellationToken);
    }
}
