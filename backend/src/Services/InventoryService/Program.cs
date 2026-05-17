using InventoryService.Api.Endpoints;
using InventoryService.Infrastructure.Consumers;
using InventoryService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Health;
using SaaSCommon.Middleware;
using Scalar.AspNetCore;
using Serilog;

namespace InventoryService;

/// <summary>
/// Entry point for the Inventory Service.
/// Manages stock levels, reservations, and releases per tenant.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        builder.Services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<InventoryReserveCommandConsumer>();
            x.AddConsumer<InventoryReleaseCommandConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(builder.Configuration["RabbitMq:Host"]!, "/", h =>
                {
                    h.Username(builder.Configuration["RabbitMq:Username"]!);
                    h.Password(builder.Configuration["RabbitMq:Password"]!);
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        builder.Services.AddOpenApi();

        builder.Services.AddStandardHealthChecks("InventoryService");

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
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

        app.MapInventoryEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "InventoryService", status = "running" }));

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            await InventoryDataSeeder.SeedAsync(dbContext, logger);
        }

        app.Run();
    }
}
