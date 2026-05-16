using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Endpoints;

/// <summary>
/// API endpoints for tenant management.
/// </summary>
public static class TenantEndpoints
{
    /// <summary>
    /// Maps tenant-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants").WithTags("Tenants").WithOpenApi();

        group.MapGet("/", async (IdentityDbContext db) =>
            Results.Ok(await db.Tenants.ToListAsync()));

        group.MapGet("/{id:guid}", async (Guid id, IdentityDbContext db) =>
            await db.Tenants.FindAsync(id) is Tenant tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapPost("/", async (CreateTenantRequest request, IdentityDbContext db) =>
        {
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Subdomain = request.Subdomain,
                SubscriptionPlanId = request.SubscriptionPlanId
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
            return Results.Created($"/api/tenants/{tenant.Id}", tenant);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTenantRequest request, IdentityDbContext db) =>
        {
            var tenant = await db.Tenants.FindAsync(id);
            if (tenant is null) return Results.NotFound();
            tenant.Name = request.Name;
            tenant.IsActive = request.IsActive;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a tenant.
/// </summary>
public record CreateTenantRequest(string Name, string Subdomain, string? SubscriptionPlanId);

/// <summary>
/// Request model for updating a tenant.
/// </summary>
public record UpdateTenantRequest(string Name, bool IsActive);
