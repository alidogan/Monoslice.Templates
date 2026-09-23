namespace Shared.Persistence;

public static class DatabaseDefaults
{
    /// <summary>Connection string shared by all modules; each module owns its own schema.</summary>
    public const string ConnectionStringName = "appdb";

    /// <summary>Schema that holds Wolverine's inbox, outbox and node tables.</summary>
    public const string MessagingSchema = "wolverine";

    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    public const string SoftDeleteFilter = "SoftDelete";
}
