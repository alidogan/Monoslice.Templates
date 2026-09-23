namespace Api.Infrastructure;

/// <summary>Adds security headers suitable for a JSON API to every response.</summary>
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Cross-Origin-Opener-Policy"] = "same-origin";
            headers["Permissions-Policy"] = "camera=(), geolocation=(), microphone=()";

            // The interactive API reference (development only) needs scripts and styles.
            if (!context.Request.Path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase))
            {
                headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
            }

            return Task.CompletedTask;
        });

        return next(context);
    }
}

internal static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}
