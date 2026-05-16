using BillingService.Domain.Entities;
using BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Api.Endpoints;

/// <summary>
/// API endpoints for subscription plan management.
/// </summary>
public static class PlanEndpoints
{
    /// <summary>
    /// Maps plan-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plans").WithTags("Plans").WithOpenApi();

        group.MapGet("/", async (BillingDbContext db) =>
            Results.Ok(await db.Plans.Where(p => p.IsActive).ToListAsync()));

        group.MapGet("/{id:guid}", async (Guid id, BillingDbContext db) =>
            await db.Plans.FindAsync(id) is Plan plan
                ? Results.Ok(plan)
                : Results.NotFound());

        group.MapPost("/", async (CreatePlanRequest request, BillingDbContext db) =>
        {
            var plan = new Plan
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                MonthlyPrice = request.MonthlyPrice,
                YearlyPrice = request.YearlyPrice,
                Currency = request.Currency,
                MaxUsers = request.MaxUsers
            };
            db.Plans.Add(plan);
            await db.SaveChangesAsync();
            return Results.Created($"/api/plans/{plan.Id}", plan);
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a plan.
/// </summary>
public record CreatePlanRequest(string Name, string? Description, decimal MonthlyPrice, decimal YearlyPrice, string Currency, int MaxUsers);
