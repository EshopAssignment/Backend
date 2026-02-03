using Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Persistence;

public sealed class DatabaseInitializerHostedService(IServiceProvider serviceProvider)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        try
        {
            var coreDb = scope.ServiceProvider.GetRequiredService<PallshoppenDbContext>();
            var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            await coreDb.Database.MigrateAsync(cancellationToken);
            await authDb.Database.MigrateAsync(cancellationToken);

            await DbSeeder.SeedAsync(coreDb, cancellationToken);

            await IdentitySeeder.SeedAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Database initialization failed");
            Console.WriteLine(ex);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
