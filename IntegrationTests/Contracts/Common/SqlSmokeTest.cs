using Microsoft.Data.SqlClient;
using IntegrationTests.Infrastructure;


namespace IntegrationTests.Contracts.Common;

public class SqlSmokeTest
{
    [Fact]
    public async Task Can_connect_to_sqlserver_with_env_connectionstring()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                 ?? throw new InvalidOperationException("Missing cs");

        EnsureDatabaseExists(cs);

        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        var result = await cmd.ExecuteScalarAsync();

        Assert.Equal(1, Convert.ToInt32(result));
    }

    private static void EnsureDatabaseExists(string cs)
    {
        var b = new SqlConnectionStringBuilder(cs);
        var db = b.InitialCatalog;

        var master = new SqlConnectionStringBuilder(cs) { InitialCatalog = "master" }.ToString();

        using var conn = new SqlConnection(master);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
        IF DB_ID(N'{db}') IS NULL
        BEGIN
            CREATE DATABASE [{db}];
        END";
        cmd.ExecuteNonQuery();
        }
}
