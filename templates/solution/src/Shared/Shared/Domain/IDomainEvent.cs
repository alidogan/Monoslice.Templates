namespace Shared.Domain;

/// <summary>
/// Marker for an event that happened inside a module. Domain events are raised by aggregates and published
/// by Wolverine through the outbox only after the surrounding transaction commits.
/// </summary>
public interface IDomainEvent;
