using CustomerService.Api.Endpoints;
using CustomerService.Infrastructure.Consumers;
using CustomerService.Infrastructure.Persistence;
using MassTransit;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Health;
using SaaSCommon.Middleware;
using Scalar.AspNetCore;
using Serilog;

namespace CustomerService;

/// <summary>
/// Entry point for the Customer Service.
/// Manages customer profiles, addresses, and order history.
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

        builder.Services.AddDbContext<CustomerDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderPlacedEventConsumer>();
            x.AddConsumer<OrderPaidEventConsumer>();

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

        builder.Services.AddOpenApi();

        builder.Services.AddStandardHealthChecks("CustomerService");

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

        app.MapCustomerEndpoints();
        app.MapAddressEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "CustomerService", status = "running" }));

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            await CustomerDataSeeder.SeedAsync(dbContext, logger);
        }

        app.Run();
    }
}
