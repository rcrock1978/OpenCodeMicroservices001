using NotificationService.Api.Endpoints;
using NotificationService.Infrastructure.Consumers;
using NotificationService.Infrastructure.Persistence;
using MassTransit;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
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
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        builder.Services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderPlacedNotificationConsumer>();
            x.AddConsumer<OrderCancelledNotificationConsumer>();
            x.AddConsumer<CustomerCreatedNotificationConsumer>();

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

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        builder.Services.AddStandardHealthChecks("NotificationService");

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

        app.MapNotificationEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "NotificationService", status = "running" }));

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            await NotificationDataSeeder.SeedAsync(dbContext, logger);
        }

        app.Run();
    }
}
