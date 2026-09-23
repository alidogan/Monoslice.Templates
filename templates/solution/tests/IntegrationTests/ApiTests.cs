using System.Net;
using IntegrationTests.Infrastructure;

namespace IntegrationTests;

public sealed class ApiTests(DatabaseFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        using var anonymous = Api.CreateClient();

        var response = await anonymous.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Responses_carry_security_headers()
    {
        var response = await Client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.Headers.GetValues("X-Content-Type-Options").ShouldBe(["nosniff"]);
        response.Headers.GetValues("X-Frame-Options").ShouldBe(["DENY"]);
        response.Headers.GetValues("Referrer-Policy").ShouldBe(["no-referrer"]);
    }
#if (UseAuth && IncludeSample)

    [Fact]
    public async Task Module_endpoints_require_authentication()
    {
        using var anonymous = Api.CreateClient();

        var response = await anonymous.GetAsync("/api/catalog/categories", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
#endif

    [Fact]
    public async Task Too_many_writes_are_rejected()
    {
        await using var api = new ApiFactory(
            Fixture.ConnectionString,
            new Dictionary<string, string?>(StringComparer.Ordinal) { ["RateLimiting:WritePermitLimit"] = "2" });
        using var client = api.CreateAuthenticatedClient();

        var statusCodes = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            // The global limiter applies to every request; health checks are excluded, so use any other path.
            using var response = await client.PostAsync("/rate-limit-probe", content: null, TestContext.Current.CancellationToken);
            statusCodes.Add(response.StatusCode);
        }

        statusCodes[^1].ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
