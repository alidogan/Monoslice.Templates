using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Api.Infrastructure;

internal sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; set; } = true;

    /// <summary>Read requests (GET, HEAD, OPTIONS) per window, per user (or per IP address when anonymous).</summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>Write requests per window, per user (or per IP address when anonymous).</summary>
    public int WritePermitLimit { get; set; } = 30;

    public int WindowSeconds { get; set; } = 60;
}

internal static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = WriteRejectionAsync;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(GetPartition);
        });

        return services;
    }

    private static RateLimitPartition<string> GetPartition(HttpContext context)
    {
        var settings = context.RequestServices.GetRequiredService<IOptionsMonitor<RateLimitingOptions>>().CurrentValue;
        if (!settings.Enabled)
        {
            return RateLimitPartition.GetNoLimiter("disabled");
        }

        var method = context.Request.Method;
        var isWrite = !HttpMethods.IsGet(method) && !HttpMethods.IsHead(method) && !HttpMethods.IsOptions(method);

        var client = context.User.Identity?.IsAuthenticated == true
            ? $"user:{context.User.FindFirstValue("sub") ?? context.User.Identity.Name}"
            : $"ip:{context.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            $"{client}:{(isWrite ? "write" : "read")}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = isWrite ? settings.WritePermitLimit : settings.PermitLimit,
                Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                QueueLimit = 0,
            });
    }

    private static async ValueTask WriteRejectionAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        var problemDetails = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetails.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context.HttpContext,
            ProblemDetails =
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too Many Requests",
                Detail = "The rate limit was exceeded. Try again later.",
            },
        });
    }
}
