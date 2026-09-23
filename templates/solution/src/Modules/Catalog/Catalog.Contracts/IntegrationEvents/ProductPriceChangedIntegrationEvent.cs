using Shared.Contracts.Messaging;

namespace Catalog.Contracts.IntegrationEvents;

/// <summary>Published after a product's price changed. Other modules subscribe by adding a handler for it.</summary>
public sealed record ProductPriceChangedIntegrationEvent(Guid ProductId, decimal OldPrice, decimal NewPrice)
    : IntegrationEvent;
