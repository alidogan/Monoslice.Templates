#if (UseRabbitMq && IncludeSample)
using System.Net;
using System.Net.Http.Json;
using Catalog.Categories.Features.CreateCategory;
using Catalog.Contracts.IntegrationEvents;
using Catalog.Products.Features.ChangeProductPrice;
using Catalog.Products.Features.CreateProduct;
using IntegrationTests.Infrastructure;
using Testcontainers.RabbitMq;
using Wolverine;
using Wolverine.Tracking;

namespace IntegrationTests.Messaging;

/// <summary>Sends an integration event through a real RabbitMQ broker.</summary>
public sealed class RabbitMqTests(DatabaseFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Integration_events_are_published_to_RabbitMQ()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var rabbitMq = new RabbitMqBuilder("rabbitmq:4-management-alpine").Build();
        await rabbitMq.StartAsync(cancellationToken);

        await using var api = new ApiFactory(
            Fixture.ConnectionString,
            messagingConnectionString: rabbitMq.GetConnectionString());
        using var client = api.CreateAuthenticatedClient();

        var category = await client.PostAsJsonAsync(
            "/api/catalog/categories",
            new CreateCategoryRequest("Games", null, 1, IsActive: true),
            cancellationToken);
        var categoryId = (await category.Content.ReadFromJsonAsync<CreateCategoryResponse>(cancellationToken))!.Id;
        var product = await client.PostAsJsonAsync(
            "/api/catalog/products",
            new CreateProductRequest(categoryId, "Chess", null, 39.95m),
            cancellationToken);
        var productId = (await product.Content.ReadFromJsonAsync<CreateProductResponse>(cancellationToken))!.Id;

        var session = await api.Services
            .TrackActivity()
            .Timeout(TimeSpan.FromSeconds(30))
            .ExecuteAndWaitAsync((Func<IMessageContext, Task>)(async _ =>
            {
                var response = await client.PutAsJsonAsync(
                    $"/api/catalog/products/{productId}/price",
                    new ChangeProductPriceRequest(49.95m),
                    cancellationToken);

                response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
            }));

        // Nothing in this application subscribes yet, so the event only leaves through the broker.
        var envelope = session.Sent.SingleEnvelope<ProductPriceChangedIntegrationEvent>();
        envelope.Destination!.Scheme.ShouldBe("rabbitmq");
    }
}
#endif
