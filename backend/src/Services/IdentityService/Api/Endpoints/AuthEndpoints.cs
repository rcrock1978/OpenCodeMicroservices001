using IdentityService.Application.Commands;
using IdentityService.Application.Queries;
using MediatR;

namespace IdentityService.Api.Endpoints;

/// <summary>
/// API endpoints for authentication.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").WithOpenApi();

        group.MapPost("/login", async (LoginRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var response = await mediator.Send(new LoginQuery(request.Email, request.Password), ct);
            return response is not null ? Results.Ok(response) : Results.Unauthorized();
        });

        group.MapPost("/register", async (RegisterRequest request, IMediator mediator, CancellationToken ct) =>
        {
            try
            {
                var result = await mediator.Send(new RegisterCommand(request.Email, request.Password, request.DisplayName, request.TenantId), ct);
                return Results.Created($"/api/users/{result.UserId}", new { result.UserId, result.Email });
            }
            catch (InvalidOperationException ex) when (ex.Message == "Email already registered")
            {
                return Results.BadRequest(ex.Message);
            }
        });

        return app;
    }
}

/// <summary>
/// Request model for user login.
/// </summary>
public record LoginRequest(string Email, string Password);

/// <summary>
/// Request model for user registration.
/// </summary>
public record RegisterRequest(string Email, string Password, string DisplayName, Guid TenantId);
