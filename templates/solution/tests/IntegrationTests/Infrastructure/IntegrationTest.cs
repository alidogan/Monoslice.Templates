namespace IntegrationTests.Infrastructure;

/// <summary>Base class that gives each test an empty database and an authenticated client.</summary>
public abstract class IntegrationTest(DatabaseFixture fixture) : IAsyncLifetime
{
    protected DatabaseFixture Fixture { get; } = fixture;

    protected ApiFactory Api => Fixture.Api;

    protected HttpClient Client { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
        Client = Api.CreateAuthenticatedClient();
    }

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
