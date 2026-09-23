#if (UseAuth)
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
#endif

namespace Api.Infrastructure;

internal static class OpenApiExtensions
{
    public static IServiceCollection AddApiOpenApi(this IServiceCollection services)
    {
#if (UseAuth)
        services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
#else
        services.AddOpenApi();
#endif
        return services;
    }
}
#if (UseAuth)

/// <summary>Documents the JWT bearer scheme so the API reference can send tokens.</summary>
internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider schemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemes = await schemeProvider.GetAllSchemesAsync();
        if (!schemes.Any(scheme => string.Equals(scheme.Name, JwtBearerDefaults.AuthenticationScheme, StringComparison.Ordinal)))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = [],
        });
    }
}
#endif
