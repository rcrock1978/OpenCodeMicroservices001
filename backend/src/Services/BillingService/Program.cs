using BillingService.Api.Endpoints;
using BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Health;
using SaaSCommon.Middleware;

namespace BillingService;

/// <summary>
/// Entry point for the Billing Service.
/// Manages subscription plans, subscriptions, and invoicing.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<BillingDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddOpenApi();

        builder.Services.AddStandardHealthChecks("BillingService");

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

        app.MapPlanEndpoints();
        app.MapSubscriptionEndpoints();
        app.MapInvoiceEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "BillingService", status = "running" }));

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            db.Database.Migrate();
        }

        app.Run();
    }
}
