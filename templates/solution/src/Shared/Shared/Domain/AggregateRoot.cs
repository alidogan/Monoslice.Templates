using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot, IAuditable, IHasConcurrencyToken
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }

    public string? CreatedBy { get; private set; }

    public DateTimeOffset? LastModifiedAt { get; private set; }

    public string? LastModifiedBy { get; private set; }

#if (UsePostgreSQL)
    public uint Version { get; private set; }
#else
    public byte[] Version { get; private set; } = [];
#endif

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
