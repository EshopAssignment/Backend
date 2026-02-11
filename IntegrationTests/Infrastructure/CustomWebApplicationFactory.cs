using Application.Interfaces;
using Infrastructure.Persistence;
using IntegrationTests.Infrastructure.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                     ?? throw new InvalidOperationException(
                         "Missing ConnectionStrings__DefaultConnection for integration tests.");

            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = cs,

                ["Stripe:SecretKey"] = "sk_test_dummy",

                ["Jwt:Key"] = "super-long-test-key-super-long-test-key-super-long-test-key",
                ["Jwt:Issuer"] = "test",
                ["Jwt:Audience"] = "test",

                ["PostNord:BaseUrl"] = "https://example.test"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                     ?? throw new InvalidOperationException(
                         "Missing ConnectionStrings__DefaultConnection for integration tests.");

            services.RemoveAll<DbContextOptions<PallshoppenDbContext>>();
            services.RemoveAll<DbContextOptions<AuthDbContext>>();

            services.AddDbContext<PallshoppenDbContext>(opt =>
                opt.UseSqlServer(cs, x =>
                {
                    x.MigrationsAssembly("Infrastructure");
                    x.MigrationsHistoryTable("__EFMigrationsHistory", "core");
                }));

            services.AddDbContext<AuthDbContext>(opt =>
                opt.UseSqlServer(cs, x =>
                {
                    x.MigrationsAssembly("Infrastructure");
                    x.MigrationsHistoryTable("__EFMigrationsHistory", "auth");
                }));

            services.RemoveAll<IHostedService>();

            services.RemoveAll<IInventoryService>();
            services.AddSingleton<IInventoryService, FakeInventoryService>();
        });
    }
}
