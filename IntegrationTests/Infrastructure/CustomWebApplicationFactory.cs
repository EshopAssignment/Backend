
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<PallshoppenDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Missing ConnectionStrings__DefaultConnection for integration tests.");

            services.AddDbContext<PallshoppenDbContext>(opt =>
                opt.UseSqlServer(cs));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PallshoppenDbContext>();
            db.Database.Migrate();
        });

        return base.CreateHost(builder);
    }
}
