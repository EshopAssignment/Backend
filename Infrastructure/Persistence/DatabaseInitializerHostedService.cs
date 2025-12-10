using Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Persistence;

public class DatabaseInitializerHostedService(IServiceProvider serviceProvider) : IHostedService
{

    //Background Service for seeding data to the database. 
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PallshoppenDbContext>();

            await db.Database.MigrateAsync(cancellationToken);
            //apply migrations
            await DbSeeder.SeedAsync(db, cancellationToken);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
            //logger not implemented. 
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
