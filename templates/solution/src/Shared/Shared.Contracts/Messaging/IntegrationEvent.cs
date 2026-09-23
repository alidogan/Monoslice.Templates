namespace Shared.Contracts.Messaging;

/// <summary>
/// An event that crosses module boundaries. Integration events live in a module's Contracts project,
/// are published through the transactional outbox and, when a broker is configured, travel over the broker.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOn { get; }
}

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
