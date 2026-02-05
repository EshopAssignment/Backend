
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Respawn;
using Respawn.Graph;

namespace IntegrationTests.Infrastructure;

public sealed class DbFixture : IAsyncLifetime
{
    private DbConnection _conn = default!;
    private Respawner _respawner = default!;

    public async Task InitializeAsync()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException("Missing Connection string");

        _conn = new SqlConnection(cs);
        await _conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(_conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = new[]
            {
                new Table("__EFMigrationsHistory")
            }
        });
    }

    public async Task ResetAsync()
    {
        await _respawner.ResetAsync(_conn);
    }

    public async Task DisposeAsync()
    {
        await _conn.DisposeAsync();
    }
}
