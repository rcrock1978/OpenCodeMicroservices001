using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace SaaSCommon.Health;

/// <summary>
/// Extension methods for configuring health checks in microservices.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds standard health checks to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The name of the service for identification.</param>
    public static IServiceCollection AddStandardHealthChecks(this IServiceCollection services, string serviceName)
    {
        services.AddHealthChecks()
            .AddCheck($"{serviceName}-self", () => HealthCheckResult.Healthy(), tags: ["live"]);

        return services;
    }

    /// <summary>
    /// Maps standard health check endpoints (/health/live and /health/ready).
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static IApplicationBuilder UseStandardHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteHealthResponse
        });

        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthResponse
        });

        return app;
    }

    private static async Task WriteHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                exception = e.Value.Exception?.Message,
                duration = e.Value.Duration.ToString()
            }),
            totalDuration = report.TotalDuration.ToString()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
