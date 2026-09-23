namespace Catalog.Products.Events;

public sealed record ProductPriceChangedDomainEvent(Guid ProductId, decimal OldPrice, decimal NewPrice) : IDomainEvent;
