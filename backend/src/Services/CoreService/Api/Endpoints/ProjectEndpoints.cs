using CoreService.Domain.Entities;
using CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreService.Api.Endpoints;

/// <summary>
/// API endpoints for project management.
/// </summary>
public static class ProjectEndpoints
{
    /// <summary>
    /// Maps project-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects").WithOpenApi();

        group.MapGet("/", async (CoreDbContext db) =>
            Results.Ok(await db.Projects.ToListAsync()));

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, CoreDbContext db) =>
            Results.Ok(await db.Projects.Where(p => p.TenantId == tenantId).ToListAsync()));

        group.MapGet("/{id:guid}", async (Guid id, CoreDbContext db) =>
            await db.Projects.FindAsync(id) is Project project
                ? Results.Ok(project)
                : Results.NotFound());

        group.MapPost("/", async (CreateProjectRequest request, CoreDbContext db) =>
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Name = request.Name,
                Description = request.Description,
                Status = ProjectStatus.Active
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            return Results.Created($"/api/projects/{project.Id}", project);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateProjectRequest request, CoreDbContext db) =>
        {
            var project = await db.Projects.FindAsync(id);
            if (project is null) return Results.NotFound();
            project.Name = request.Name;
            project.Description = request.Description;
            project.Status = request.Status;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, CoreDbContext db) =>
        {
            var project = await db.Projects.FindAsync(id);
            if (project is null) return Results.NotFound();
            db.Projects.Remove(project);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a project.
/// </summary>
public record CreateProjectRequest(Guid TenantId, string Name, string? Description);

/// <summary>
/// Request model for updating a project.
/// </summary>
public record UpdateProjectRequest(string Name, string? Description, ProjectStatus Status);
