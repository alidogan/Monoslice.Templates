namespace Shared.Contracts.Messaging;

/// <summary>Marker for a message that changes state and returns <typeparamref name="TResult"/>.</summary>
public interface ICommand<TResult>;

/// <summary>Marker for a message that changes state without a result value.</summary>
public interface ICommand;
