namespace Api.Infrastructure;

internal static class CorsExtensions
{
    /// <summary>Allows the origins listed in <c>Cors:AllowedOrigins</c>; no origins are allowed by default.</summary>
    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            if (origins.Length > 0)
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("ETag", "Location");
            }
        }));

        return services;
    }
}
