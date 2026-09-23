using System.Data.Common;
#if (UsePostgreSQL)
using Npgsql;
#else
using Microsoft.Data.SqlClient;
#endif
using Respawn;
#if (UsePostgreSQL)
using Testcontainers.PostgreSql;
#else
using Testcontainers.MsSql;
#endif

namespace IntegrationTests.Infrastructure;

/// <summary>
/// Starts the database container, the API (which applies all module migrations) and resets
/// module data between tests with Respawn.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
#if (UsePostgreSQL)
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();
#else
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
#endif
    private Respawner? _respawner;

    public string ConnectionString { get; private set; } = string.Empty;

    public ApiFactory Api { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
#if (UsePostgreSQL)
        ConnectionString = _container.GetConnectionString();
#else
        ConnectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "appdb",
        }.ConnectionString;
#endif

        Api = new ApiFactory(ConnectionString);
        await Api.StartAsync();

        await using var connection = await OpenConnectionAsync();
        try
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
#if (UsePostgreSQL)
                DbAdapter = DbAdapter.Postgres,
#else
                DbAdapter = DbAdapter.SqlServer,
#endif
                SchemasToExclude = ["wolverine"],
                TablesToIgnore = ["__EFMigrationsHistory"],
            });
        }
        catch (InvalidOperationException)
        {
            // No module has tables yet, so there is nothing to reset.
            _respawner = null;
        }
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null)
        {
            return;
        }

        await using var connection = await OpenConnectionAsync();
        await _respawner.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        await Api.DisposeAsync();
        await _container.DisposeAsync();
    }

    private async Task<DbConnection> OpenConnectionAsync()
    {
#if (UsePostgreSQL)
        var connection = new NpgsqlConnection(ConnectionString);
#else
        var connection = new SqlConnection(ConnectionString);
#endif
        await connection.OpenAsync();
        return connection;
    }
}
