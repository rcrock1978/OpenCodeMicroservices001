using SaaSCommon.Middleware;

namespace Gateway;

/// <summary>
/// Entry point for the API Gateway service.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        builder.Services.AddOpenApi();

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddOtlpExporter();
            });

        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseOpenApi();

        app.MapReverseProxy();

        app.MapGet("/", () => Results.Ok(new { service = "Gateway", status = "running" }));

        app.Run();
    }
}
