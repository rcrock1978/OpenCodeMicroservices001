using NotificationService.Api.Endpoints;
using NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Health;
using SaaSCommon.Middleware;
using Scalar.AspNetCore;
using Serilog;

namespace NotificationService;

/// <summary>
/// Entry point for the Notification Service.
/// Consumes domain events and dispatches email/SMS/webhook notifications.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        builder.Services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddOpenApi();

        builder.Services.AddStandardHealthChecks("NotificationService");

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddEntityFrameworkCoreInstrumentation()
                       .AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation();
            });

        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();

        app.MapOpenApi();
        app.MapScalarApiReference();

        app.UseStandardHealthChecks();

        app.MapNotificationEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "NotificationService", status = "running" }));

        app.Run();
    }
}
