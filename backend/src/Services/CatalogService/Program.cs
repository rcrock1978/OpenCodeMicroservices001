using CatalogService.Api.Endpoints;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Health;
using SaaSCommon.Middleware;
using Scalar.AspNetCore;
using Serilog;

namespace CatalogService;

/// <summary>
/// Entry point for the Catalog Service.
/// Manages products, variants, categories, and media assets.
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

        builder.Services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddStackExchangeRedisCache(options =>
            options.Configuration = builder.Configuration.GetConnectionString("Redis"));

        builder.Services.AddOpenApi();

        builder.Services.AddStandardHealthChecks("CatalogService");

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

        app.MapProductEndpoints();
        app.MapCategoryEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "CatalogService", status = "running" }));

        app.Run();
    }
}
