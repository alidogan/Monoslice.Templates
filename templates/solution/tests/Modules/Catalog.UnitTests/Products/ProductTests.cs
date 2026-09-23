using Catalog.Products.Events;
using Catalog.Products.Models;
using Shared.Contracts.Results;

namespace Catalog.UnitTests.Products;

public sealed class ProductTests
{
    private static Product CreateProduct(decimal price = 10m) =>
        Product.Create(Guid.CreateVersion7(), Guid.CreateVersion7(), "Chess", null, price);

    [Fact]
    public void ChangePrice_raises_a_domain_event()
    {
        var product = CreateProduct(price: 10m);

        var result = product.ChangePrice(12.5m);

        result.IsSuccess.ShouldBeTrue();
        product.Price.ShouldBe(12.5m);
        var domainEvent = product.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ProductPriceChangedDomainEvent>();
        domainEvent.ShouldBe(new ProductPriceChangedDomainEvent(product.Id, 10m, 12.5m));
    }

    [Fact]
    public void ChangePrice_to_the_same_price_does_nothing()
    {
        var product = CreateProduct(price: 10m);

        var result = product.ChangePrice(10m);

        result.IsSuccess.ShouldBeTrue();
        product.DomainEvents.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangePrice_rejects_a_price_that_is_not_positive(decimal price)
    {
        var product = CreateProduct(price: 10m);

        var result = product.ChangePrice(price);

        result.IsFailure.ShouldBeTrue();
        result.Error.Type.ShouldBe(ErrorType.Validation);
        product.Price.ShouldBe(10m);
        product.DomainEvents.ShouldBeEmpty();
    }
}
