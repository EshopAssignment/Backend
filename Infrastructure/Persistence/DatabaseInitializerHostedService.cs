using Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public sealed class DatabaseInitializerHostedService(
    IServiceProvider serviceProvider,
    ILogger<DatabaseInitializerHostedService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 30;            
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                var coreDb = scope.ServiceProvider.GetRequiredService<PallshoppenDbContext>();
                var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

                logger.LogInformation("Database init attempt {Attempt}/{MaxAttempts}...", attempt, maxAttempts);

                await coreDb.Database.MigrateAsync(cancellationToken);
                await authDb.Database.MigrateAsync(cancellationToken);

                await DbSeeder.SeedAsync(coreDb, cancellationToken);

                await IdentitySeeder.SeedAsync(scope.ServiceProvider);

                logger.LogInformation("Database initialization succeeded");
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Database initialization cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                if (attempt >= maxAttempts)
                {
                    logger.LogError(ex, "Database initialization failed after {MaxAttempts} attempts", maxAttempts);
                    throw;
                }

                logger.LogWarning(ex,
                    " Database initialization failed (attempt {Attempt}/{MaxAttempts}). Retrying in {DelaySeconds}s...",
                    attempt, maxAttempts, delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
