using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Endpoints;
using NotificationService.Infrastructure.Persistence;
using SaaSCommon.Health;
using SaaSCommon.Middleware;

namespace NotificationService;

/// <summary>
/// Entry point for the Notification Service.
/// Manages email templates, notifications, and webhook deliveries.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
            });

        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseOpenApi();

        app.UseStandardHealthChecks();

        app.MapNotificationEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "NotificationService", status = "running" }));

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            db.Database.Migrate();
        }

        app.Run();
    }
}
