using Catalog.Products.Errors;
using Catalog.Products.Events;

namespace Catalog.Products.Models;

public sealed class Product : AggregateRoot<Guid>
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 4000;

    private Product()
    {
    }

    private Product(Guid id, Guid categoryId, string name, string? description, decimal price)
        : base(id)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
    }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public static Product Create(Guid id, Guid categoryId, string name, string? description, decimal price) =>
        new(id, categoryId, name.Trim(), description?.Trim(), price);

    public Result ChangePrice(decimal newPrice)
    {
        if (newPrice <= 0)
        {
            return ProductErrors.InvalidPrice(newPrice);
        }

        if (newPrice == Price)
        {
            return Result.Success();
        }

        var oldPrice = Price;
        Price = newPrice;
        Raise(new ProductPriceChangedDomainEvent(Id, oldPrice, newPrice));

        return Result.Success();
    }
}
