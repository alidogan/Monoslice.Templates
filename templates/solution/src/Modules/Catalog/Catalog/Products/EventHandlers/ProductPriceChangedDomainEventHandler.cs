using Catalog.Contracts.IntegrationEvents;
using Catalog.Products.Events;

namespace Catalog.Products.EventHandlers;

/// <summary>
/// Translates the internal domain event into the public integration event. The returned message is
/// published by Wolverine (a "cascading message"), through the outbox.
/// </summary>
public static class ProductPriceChangedDomainEventHandler
{
    public static ProductPriceChangedIntegrationEvent Handle(ProductPriceChangedDomainEvent domainEvent) =>
        new(domainEvent.ProductId, domainEvent.OldPrice, domainEvent.NewPrice);
}
