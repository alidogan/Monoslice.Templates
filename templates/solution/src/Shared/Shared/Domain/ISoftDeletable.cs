namespace Shared.Domain;

/// <summary>
/// Opt-in soft delete. Removing such an entity marks it as deleted (<c>SoftDeleteInterceptor</c>)
/// and a global query filter hides it from queries.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTimeOffset? DeletedAt { get; }

    string? DeletedBy { get; }
}
