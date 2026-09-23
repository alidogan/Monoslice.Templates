var builder = DistributedApplication.CreateBuilder(args);

#if (UsePostgreSQL)
var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("appdb");
#else
var database = builder.AddSqlServer("sqlserver")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("appdb");
#endif

var cache = builder.AddRedis("cache");

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health")
    .WithUrlForEndpoint("http", url => url.DisplayText = "API")
    .WithUrl("/scalar", "API reference");
#if (UseAuth)

// Realm "app" with the "api" audience and a development user alice / alice (see Realms/app-realm.json).
var keycloak = builder.AddKeycloak("keycloak", port: 8080)
    .WithDataVolume()
    .WithRealmImport("./Realms")
    .WithLifetime(ContainerLifetime.Persistent);

api.WaitFor(keycloak)
    .WithEnvironment("Authentication__Authority", ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/realms/app"))
    .WithEnvironment("Authentication__RequireHttpsMetadata", "false");
#endif
#if (UseRabbitMq)

var messaging = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

api.WithReference(messaging).WaitFor(messaging);
#elif (UseAzureServiceBus)

var messaging = builder.AddAzureServiceBus("messaging").RunAsEmulator();

// The emulator serves its management API on the health port; Wolverine uses it to create queues.
var emulatorManagement = messaging.GetEndpoint("emulatorhealth");
api.WithReference(messaging)
    .WaitFor(messaging)
    .WithEnvironment(
        "Messaging__ManagementConnectionString",
        ReferenceExpression.Create(
            $"Endpoint=sb://{emulatorManagement.Property(EndpointProperty.HostAndPort)};SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"));
#endif

await builder.Build().RunAsync();
