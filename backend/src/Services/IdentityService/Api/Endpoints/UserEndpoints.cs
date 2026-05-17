using IdentityService.Application.Commands;
using IdentityService.Application.Queries;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Api.Endpoints;

/// <summary>
/// API endpoints for user management.
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Maps user-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").WithOpenApi();

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var users = await mediator.Send(new GetUsersQuery(), ct);
            return Results.Ok(users.Select(MapToResponse));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            await mediator.Send(new GetUserByIdQuery(id), ct) is { } user
                ? Results.Ok(MapToResponse(user))
                : Results.NotFound());

        group.MapPost("/", async (CreateUserRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var user = await mediator.Send(new CreateUserCommand(request.Email, request.Password, request.DisplayName, request.TenantId, request.Role), ct);
            return Results.Created($"/api/users/{user.Id}", MapToResponse(user));
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteUserCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }

    private static UserResponse MapToResponse(User user) =>
        new(
            user.Id,
            user.Email,
            user.DisplayName,
            user.TenantId,
            user.Tenant is null ? null : new TenantSummaryResponse(
                user.Tenant.Id,
                user.Tenant.Name,
                user.Tenant.Subdomain,
                user.Tenant.SubscriptionPlanId,
                user.Tenant.CreatedAt,
                user.Tenant.IsActive),
            user.Role,
            user.CreatedAt,
            user.IsActive
        );
}

/// <summary>
/// Request model for creating a user.
/// </summary>
public record CreateUserRequest(string Email, string Password, string DisplayName, Guid TenantId, UserRole Role);
