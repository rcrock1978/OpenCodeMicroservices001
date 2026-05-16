using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

        group.MapGet("/", async (IdentityDbContext db) =>
            Results.Ok(await db.Users.Include(u => u.Tenant).ToListAsync()));

        group.MapGet("/{id:guid}", async (Guid id, IdentityDbContext db) =>
            await db.Users.Include(u => u.Tenant).FirstOrDefaultAsync(u => u.Id == id) is User user
                ? Results.Ok(user)
                : Results.NotFound());

        group.MapPost("/", async (CreateUserRequest request, IdentityDbContext db) =>
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DisplayName = request.DisplayName,
                TenantId = request.TenantId,
                Role = request.Role
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Results.Created($"/api/users/{user.Id}", user);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IdentityDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound();
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a user.
/// </summary>
public record CreateUserRequest(string Email, string Password, string DisplayName, Guid TenantId, UserRole Role);
