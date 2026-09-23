using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api.Infrastructure;

internal static class AuthenticationExtensions
{
    /// <summary>
    /// JWT bearer authentication against any OpenID Connect provider (Keycloak, Microsoft Entra ID, Auth0, ...),
    /// configured through the <c>Authentication</c> section.
    /// </summary>
    public static WebApplicationBuilder AddApiAuthentication(this WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection("Authentication");

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = section["Authority"];
                options.Audience = section["Audience"];
                options.RequireHttpsMetadata = section.GetValue("RequireHttpsMetadata", defaultValue: true);
                options.MapInboundClaims = false;
                options.TokenValidationParameters.NameClaimType = "preferred_username";
                options.TokenValidationParameters.RoleClaimType = "roles";

                // When the API reaches the identity provider on another address than clients do
                // (e.g. inside docker compose), read the metadata there but keep validating the public issuer.
                if (section["MetadataAddress"] is { Length: > 0 } metadataAddress)
                {
                    options.MetadataAddress = metadataAddress;
                    options.TokenValidationParameters.ValidIssuer = section["Authority"];
                }
            });

        builder.Services.AddAuthorization();
        return builder;
    }
}
