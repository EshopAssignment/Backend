using System.Data.Common;
using Domain.Entities.Identity;
using Infrastructure.Persistence;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

namespace IntegrationTests.Infrastructure;

public sealed class DbFixture : IAsyncLifetime
{
    private DbConnection _conn = default!;
    private Respawner _respawner = default!;

    public async Task InitializeAsync()
    {
        var cs = GetConnectionString();

        EnsureDatabaseExists(cs);
        await MigrateAsync(cs);

        _conn = new SqlConnection(cs);
        await _conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(_conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = new[] { "core", "auth", "dbo" },
            TablesToIgnore = new[]
            {
                new Table("__EFMigrationsHistory", "core"),
                new Table("__EFMigrationsHistory", "auth")
            }
        });

        await SeedAsync(cs);
    }

    public async Task ResetAsync()
    {
        await _respawner.ResetAsync(_conn);
        await SeedAsync(GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        if (_conn is not null)
            await _conn.DisposeAsync();
    }

    private static string GetConnectionString()
        => Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
           ?? throw new InvalidOperationException("Missing ConnectionStrings__DefaultConnection");

    private static void EnsureDatabaseExists(string cs)
    {
        var b = new SqlConnectionStringBuilder(cs);
        var dbName = b.InitialCatalog;

        if (string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException("Connection string must include Initial Catalog / Database.");

        var master = new SqlConnectionStringBuilder(cs) { InitialCatalog = "master" }.ToString();

        using var conn = new SqlConnection(master);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            IF DB_ID(N'{dbName}') IS NULL
            BEGIN
            CREATE DATABASE [{dbName}];
            END";
        cmd.ExecuteNonQuery();
    }

    private static async Task MigrateAsync(string cs)
    {
        await using var sp = BuildTestServiceProvider(cs);
        await using var scope = sp.CreateAsyncScope();

        var coreDb = scope.ServiceProvider.GetRequiredService<PallshoppenDbContext>();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await coreDb.Database.MigrateAsync();
        await authDb.Database.MigrateAsync();
    }

    private static async Task SeedAsync(string cs)
    {
        await using var sp = BuildTestServiceProvider(cs);
        await using var scope = sp.CreateAsyncScope();

        var coreDb = scope.ServiceProvider.GetRequiredService<PallshoppenDbContext>();

        await DbSeeder.SeedAsync(coreDb);
        await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    }

    private static ServiceProvider BuildTestServiceProvider(string cs)
    {
        var services = new ServiceCollection();

        services.AddDbContext<PallshoppenDbContext>(opt =>
            opt.UseSqlServer(cs, x =>
            {
                x.MigrationsAssembly("Infrastructure");
                x.MigrationsHistoryTable("__EFMigrationsHistory", "core");
                x.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
            }));

        services.AddDbContext<AuthDbContext>(opt =>
            opt.UseSqlServer(cs, x =>
            {
                x.MigrationsAssembly("Infrastructure");
                x.MigrationsHistoryTable("__EFMigrationsHistory", "auth");
                x.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
            }));

        services.AddDataProtection();

        services
            .AddIdentityCore<User>()
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }
}
