using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence;

public static class DatabaseProvider
{
    /// <summary>Configures the database provider for a module DbContext that lives in <paramref name="schema"/>.</summary>
    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder options, string connectionString, string schema) =>
#if (UsePostgreSQL)
        options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable(DatabaseDefaults.MigrationsHistoryTable, schema));
#else
        options.UseSqlServer(
            connectionString,
            sqlServer => sqlServer.MigrationsHistoryTable(DatabaseDefaults.MigrationsHistoryTable, schema));
#endif
}
