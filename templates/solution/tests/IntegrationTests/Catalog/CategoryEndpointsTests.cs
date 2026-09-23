using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Catalog.Categories.Dtos;
using Catalog.Categories.Features.CreateCategory;
using Catalog.Categories.Features.UpdateCategory;
using Catalog.Data;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Pagination;

namespace IntegrationTests.Catalog;

public sealed class CategoryEndpointsTests(DatabaseFixture fixture) : IntegrationTest(fixture)
{
    private const string Categories = "/api/catalog/categories";

    [Fact]
    public async Task Create_then_get_returns_the_category_with_an_etag()
    {
        var created = await CreateCategoryAsync("Books");

        var response = await Client.GetAsync($"{Categories}/{created.Id}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag.ShouldNotBeNull();
        var category = await response.Content.ReadFromJsonAsync<CatalogCategoryDto>(TestContext.Current.CancellationToken);
        category.ShouldBe(new CatalogCategoryDto(created.Id, "Books", "Description of Books", 1, IsActive: true));
    }

    [Fact]
    public async Task Create_with_invalid_data_returns_validation_problem()
    {
        var response = await Client.PostAsJsonAsync(
            Categories,
            new CreateCategoryRequest(string.Empty, null, -1, IsActive: true),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
        problem.ShouldNotBeNull();
        problem.Errors.Keys.ShouldBe(["Name", "DisplayOrder"], ignoreOrder: true);
    }

    [Fact]
    public async Task Create_with_a_duplicate_name_returns_conflict()
    {
        await CreateCategoryAsync("Books");

        var response = await Client.PostAsJsonAsync(
            Categories,
            new CreateCategoryRequest("Books", null, 2, IsActive: true),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Get_an_unknown_category_returns_not_found()
    {
        var response = await Client.GetAsync($"{Categories}/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        problem!.Extensions["code"]!.ToString().ShouldBe("Catalog.Category.NotFound");
    }

    [Fact]
    public async Task List_returns_categories_page_by_page()
    {
        for (var i = 1; i <= 3; i++)
        {
            await CreateCategoryAsync($"Category {i}", displayOrder: i);
        }

        var page = await Client.GetFromJsonAsync<PagedResult<CatalogCategoryDto>>(
            $"{Categories}?page=2&pageSize=2",
            TestContext.Current.CancellationToken);

        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(3);
        page.TotalPages.ShouldBe(2);
        page.Items.ShouldHaveSingleItem().Name.ShouldBe("Category 3");
    }

    [Fact]
    public async Task Update_with_the_current_etag_succeeds()
    {
        var created = await CreateCategoryAsync("Books");
        var eTag = await GetETagAsync(created.Id);

        var response = await SendUpdateAsync(created.Id, eTag, "Novels");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var category = await Client.GetFromJsonAsync<CatalogCategoryDto>(
            $"{Categories}/{created.Id}",
            TestContext.Current.CancellationToken);
        category!.Name.ShouldBe("Novels");
    }

    [Fact]
    public async Task Update_with_a_stale_etag_returns_precondition_failed()
    {
        var created = await CreateCategoryAsync("Books");
        var staleETag = await GetETagAsync(created.Id);
        (await SendUpdateAsync(created.Id, staleETag, "Novels")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var response = await SendUpdateAsync(created.Id, staleETag, "Poetry");

        response.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
    }

    [Fact]
    public async Task Update_without_if_match_returns_precondition_required()
    {
        var created = await CreateCategoryAsync("Books");

        var response = await Client.PutAsJsonAsync(
            $"{Categories}/{created.Id}",
            new UpdateCategoryRequest("Novels", null, 1, IsActive: true),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.PreconditionRequired);
    }

    [Fact]
    public async Task Delete_soft_deletes_the_category()
    {
        var created = await CreateCategoryAsync("Books");
        var eTag = await GetETagAsync(created.Id);

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{Categories}/{created.Id}");
        request.Headers.IfMatch.Add(EntityTagHeaderValue.Parse(eTag));
        var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await Client.GetAsync($"{Categories}/{created.Id}", TestContext.Current.CancellationToken))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await using var scope = Api.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var row = await dbContext.Categories
            .IgnoreQueryFilters()
            .SingleAsync(category => category.Id == created.Id, TestContext.Current.CancellationToken);
        row.IsDeleted.ShouldBeTrue();
        row.DeletedAt.ShouldNotBeNull();
    }

    private async Task<CreateCategoryResponse> CreateCategoryAsync(string name, int displayOrder = 1)
    {
        var response = await Client.PostAsJsonAsync(
            Categories,
            new CreateCategoryRequest(name, $"Description of {name}", displayOrder, IsActive: true),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        return (await response.Content.ReadFromJsonAsync<CreateCategoryResponse>(TestContext.Current.CancellationToken))!;
    }

    private async Task<string> GetETagAsync(Guid id)
    {
        var response = await Client.GetAsync($"{Categories}/{id}", TestContext.Current.CancellationToken);
        return response.Headers.ETag!.ToString();
    }

    private async Task<HttpResponseMessage> SendUpdateAsync(Guid id, string eTag, string name)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"{Categories}/{id}")
        {
            Content = JsonContent.Create(new UpdateCategoryRequest(name, null, 1, IsActive: true)),
        };
        request.Headers.IfMatch.Add(EntityTagHeaderValue.Parse(eTag));
        return await Client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
