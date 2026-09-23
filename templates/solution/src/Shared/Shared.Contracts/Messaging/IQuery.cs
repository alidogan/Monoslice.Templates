namespace Shared.Contracts.Messaging;

/// <summary>Marker for a read-only message that returns <typeparamref name="TResult"/>.</summary>
public interface IQuery<TResult>;
