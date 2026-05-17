using IdentityService.Api.Endpoints;
using IdentityService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Health;
using SaaSCommon.Middleware;
using Scalar.AspNetCore;
using Serilog;

namespace IdentityService;

/// <summary>
/// Entry point for the Identity Service.
/// Manages merchant and customer identities, tenant isolation, and stub JWT issuance.
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

        builder.Services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        builder.Services.AddStackExchangeRedisCache(options =>
            options.Configuration = builder.Configuration.GetConnectionString("Redis"));

        builder.Services.AddMassTransit(x =>
        {
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

        builder.Services.AddAuthentication()
            .AddJwtBearer();

        builder.Services.AddAuthorization();

        builder.Services.AddOpenApi();

        builder.Services.AddStandardHealthChecks("IdentityService");

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

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            await IdentityDbContextSeed.SeedAsync(context);
        }

        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapOpenApi();
        app.MapScalarApiReference();

        app.UseStandardHealthChecks();

        app.MapAuthEndpoints();
        app.MapTenantEndpoints();
        app.MapUserEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "IdentityService", status = "running" }));

        app.Run();
    }
}
