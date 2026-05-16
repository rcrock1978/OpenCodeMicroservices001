using CoreService.Api.Endpoints;
using CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Health;
using SaaSCommon.Middleware;

namespace CoreService;

/// <summary>
/// Entry point for the Core Service.
/// Placeholder for the main business domain logic of the SaaS platform.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<CoreDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddOpenApi();

        builder.Services.AddStandardHealthChecks("CoreService");

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddEntityFrameworkCoreInstrumentation()
                       .AddOtlpExporter();
            });

        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseOpenApi();

        app.UseStandardHealthChecks();

        app.MapProjectEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "CoreService", status = "running" }));

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
            db.Database.Migrate();
        }

        app.Run();
    }
}
