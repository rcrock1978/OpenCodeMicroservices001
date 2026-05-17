using PaymentService.Api.Endpoints;
using PaymentService.Infrastructure.Consumers;
using PaymentService.Infrastructure.Persistence;
using MassTransit;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Health;
using SaaSCommon.Middleware;
using Scalar.AspNetCore;
using Serilog;

namespace PaymentService;

/// <summary>
/// Entry point for the Payment Service.
/// Simulates payment processing, idempotency, and webhook contracts.
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

        builder.Services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<PaymentInitiateCommandConsumer>();
            x.AddConsumer<RefundPaymentCommandConsumer>();

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

        builder.Services.AddStandardHealthChecks("PaymentService");

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

        app.MapPaymentEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "PaymentService", status = "running" }));

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            await PaymentDataSeeder.SeedAsync(dbContext, logger);
        }

        app.Run();
    }
}
