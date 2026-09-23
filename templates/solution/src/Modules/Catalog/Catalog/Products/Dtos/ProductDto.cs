namespace Catalog.Products.Dtos;

public sealed record ProductDto(Guid Id, Guid CategoryId, string Name, string? Description, decimal Price);
