using JasperFx.CommandLine;
#if (UseAuth)
using Microsoft.AspNetCore.Authentication;
#endif
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
#if (UseAuth)
using Microsoft.Extensions.DependencyInjection;
#endif
#if (UseBroker)
using Wolverine;
#endif

namespace IntegrationTests.Infrastructure;

/// <summary>Hosts the real API in memory against the test database.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
#if (UseRabbitMq)
    private const string UnusedMessagingConnectionString = "amqp://guest:guest@localhost:5672";
#elif (UseAzureServiceBus)
    private const string UnusedMessagingConnectionString =
        "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
#endif
    private readonly Dictionary<string, string?> _settings;
#if (UseBroker)
    private readonly bool _useBroker;
#endif

#if (UseBroker)
    /// <summary>
    /// Creates the API. Pass <paramref name="messagingConnectionString"/> to use a real broker;
    /// by default external transports are stubbed.
    /// </summary>
    public ApiFactory(
        string connectionString,
        IDictionary<string, string?>? settings = null,
        string? messagingConnectionString = null)
#else
    public ApiFactory(string connectionString, IDictionary<string, string?>? settings = null)
#endif
    {
        // Wolverine's command line support should start the host when used by WebApplicationFactory.
        JasperFxEnvironment.AutoStartHost = true;

        // Read while the application builds its services, so they are passed as environment variables.
        Environment.SetEnvironmentVariable("ConnectionStrings__appdb", connectionString);
#if (UseBroker)
        _useBroker = messagingConnectionString is not null;
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__messaging",
            messagingConnectionString ?? UnusedMessagingConnectionString);
#endif

        _settings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Database:MigrateOnStartup"] = "true",
            ["Database:SeedOnStartup"] = "false",
            ["RateLimiting:PermitLimit"] = "10000",
            ["RateLimiting:WritePermitLimit"] = "10000",
        };

        foreach (var (key, value) in settings ?? new Dictionary<string, string?>(StringComparer.Ordinal))
        {
            _settings[key] = value;
        }
    }

    /// <summary>Starts the host, which applies all module migrations.</summary>
    public Task StartAsync()
    {
        _ = Services;
        return Task.CompletedTask;
    }

#if (UseAuth)
    /// <summary>A client that is authenticated as <paramref name="userId"/>.</summary>
    public HttpClient CreateAuthenticatedClient(string userId = "alice")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new(TestAuthHandler.SchemeName, userId);
        return client;
    }
#else
    public HttpClient CreateAuthenticatedClient() => CreateClient();
#endif

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(_settings));

        builder.ConfigureTestServices(services =>
        {
#if (UseAuth)
            services
                .AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
#endif
#if (UseBroker)

            // Integration tests do not need the broker: outgoing messages are recorded instead of sent.
            if (!_useBroker)
            {
                services.DisableAllExternalWolverineTransports();
            }
#endif
        });
    }
}
