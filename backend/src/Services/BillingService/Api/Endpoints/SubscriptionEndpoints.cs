using BillingService.Domain.Entities;
using BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Api.Endpoints;

/// <summary>
/// API endpoints for subscription management.
/// </summary>
public static class SubscriptionEndpoints
{
    /// <summary>
    /// Maps subscription-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapSubscriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/subscriptions").WithTags("Subscriptions").WithOpenApi();

        group.MapGet("/", async (BillingDbContext db) =>
            Results.Ok(await db.Subscriptions.Include(s => s.Plan).ToListAsync()));

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, BillingDbContext db) =>
            Results.Ok(await db.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.TenantId == tenantId)
                .ToListAsync()));

        group.MapPost("/", async (CreateSubscriptionRequest request, BillingDbContext db) =>
        {
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                PlanId = request.PlanId,
                StartDate = request.StartDate,
                Interval = request.Interval,
                Status = SubscriptionStatus.Active
            };
            db.Subscriptions.Add(subscription);
            await db.SaveChangesAsync();
            return Results.Created($"/api/subscriptions/{subscription.Id}", subscription);
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, BillingDbContext db) =>
        {
            var subscription = await db.Subscriptions.FindAsync(id);
            if (subscription is null) return Results.NotFound();
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.EndDate = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a subscription.
/// </summary>
public record CreateSubscriptionRequest(Guid TenantId, Guid PlanId, DateTime StartDate, BillingInterval Interval);
