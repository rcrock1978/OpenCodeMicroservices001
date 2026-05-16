using IdentityService.Api.Endpoints;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Health;
using SaaSCommon.Middleware;

namespace IdentityService;

/// <summary>
/// Entry point for the Identity Service.
/// Manages users, tenants, authentication, and authorization.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddStackExchangeRedisCache(options =>
            options.Configuration = builder.Configuration.GetConnectionString("Redis"));

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
                       .AddEntityFrameworkCoreInstrumentation()
                       .AddOtlpExporter();
            });

        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseOpenApi();

        app.UseStandardHealthChecks();

        app.MapAuthEndpoints();
        app.MapTenantEndpoints();
        app.MapUserEndpoints();

        app.MapGet("/", () => Results.Ok(new { service = "IdentityService", status = "running" }));

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.Database.Migrate();
        }

        app.Run();
    }
}
