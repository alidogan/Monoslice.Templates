namespace Shared.Domain;

/// <summary>Audit columns, populated by <c>AuditableEntityInterceptor</c>.</summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }

    string? CreatedBy { get; }

    DateTimeOffset? LastModifiedAt { get; }

    string? LastModifiedBy { get; }
}
