using System.Net;
using System.Net.Http.Json;
using Catalog.Categories.Features.CreateCategory;
using Catalog.Contracts.Categories;
using Catalog.Contracts.IntegrationEvents;
using Catalog.Products.Dtos;
using Catalog.Products.Features.ChangeProductPrice;
using Catalog.Products.Features.CreateProduct;
using IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Pagination;
using Shared.Contracts.Results;
using Wolverine;
using Wolverine.Tracking;

namespace IntegrationTests.Catalog;

public sealed class ProductEndpointsTests(DatabaseFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Create_product_in_an_unknown_category_returns_bad_request()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/catalog/products",
            new CreateProductRequest(Guid.CreateVersion7(), "Chess", null, 10m),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Created_products_are_listed_by_category()
    {
        var categoryId = await CreateCategoryAsync();
        await CreateProductAsync(categoryId, "Chess", 39.95m);

        var page = await Client.GetFromJsonAsync<PagedResult<ProductDto>>(
            $"/api/catalog/products?categoryId={categoryId}",
            TestContext.Current.CancellationToken);

        page!.Items.ShouldHaveSingleItem().Name.ShouldBe("Chess");
    }

    [Fact]
    public async Task Changing_the_price_publishes_an_integration_event()
    {
        var categoryId = await CreateCategoryAsync();
        var productId = await CreateProductAsync(categoryId, "Chess", 39.95m);

        // Tracks every message Wolverine handles until all activity (including the outbox) is done.
        var session = await Api.Services
            .TrackActivity()
            .Timeout(TimeSpan.FromSeconds(30))
            .ExecuteAndWaitAsync((Func<IMessageContext, Task>)(async _ =>
            {
                var response = await Client.PutAsJsonAsync(
                    $"/api/catalog/products/{productId}/price",
                    new ChangeProductPriceRequest(49.95m),
                    TestContext.Current.CancellationToken);

                response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
            }));

        var integrationEvent = session.FindSingleTrackedMessageOfType<ProductPriceChangedIntegrationEvent>();
        integrationEvent.ShouldNotBeNull();
        integrationEvent.ProductId.ShouldBe(productId);
        integrationEvent.OldPrice.ShouldBe(39.95m);
        integrationEvent.NewPrice.ShouldBe(49.95m);
    }

    [Fact]
    public async Task Other_modules_can_query_a_category_through_the_contract()
    {
        var categoryId = await CreateCategoryAsync();
        var bus = Api.Services.GetRequiredService<IMessageBus>();

        var result = await bus.InvokeAsync<Result<CategorySummaryDto>>(
            new GetCategorySummaryQuery(categoryId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Games");
    }

    private async Task<Guid> CreateCategoryAsync()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/catalog/categories",
            new CreateCategoryRequest("Games", null, 1, IsActive: true),
            TestContext.Current.CancellationToken);

        return (await response.Content.ReadFromJsonAsync<CreateCategoryResponse>(TestContext.Current.CancellationToken))!.Id;
    }

    private async Task<Guid> CreateProductAsync(Guid categoryId, string name, decimal price)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/catalog/products",
            new CreateProductRequest(categoryId, name, null, price),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CreateProductResponse>(TestContext.Current.CancellationToken))!.Id;
    }
}
