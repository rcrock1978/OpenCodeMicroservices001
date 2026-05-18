using IdentityService.Application.Commands;
using IdentityService.Application.Queries;
using MediatR;

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

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetTenantsQuery(), ct)));

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            await mediator.Send(new GetTenantByIdQuery(id), ct) is { } tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapGet("/by-subdomain/{subdomain}", async (string subdomain, IMediator mediator, CancellationToken ct) =>
            await mediator.Send(new GetTenantBySubdomainQuery(subdomain), ct) is { } tenant
                ? Results.Ok(tenant)
                : Results.NotFound());

        group.MapPost("/", async (CreateTenantRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var tenant = await mediator.Send(new CreateTenantCommand(request.Name, request.Subdomain, request.SubscriptionPlanId), ct);
            return Results.Created($"/api/tenants/{tenant.Id}", tenant);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTenantRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var tenant = await mediator.Send(new UpdateTenantCommand(id, request.Name, request.IsActive), ct);
            return tenant is not null ? Results.NoContent() : Results.NotFound();
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
