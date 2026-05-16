using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Api.Endpoints;

/// <summary>
/// API endpoints for notification management.
/// </summary>
public static class NotificationEndpoints
{
    /// <summary>
    /// Maps notification-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications").WithOpenApi();

        group.MapGet("/", async (NotificationDbContext db) =>
            Results.Ok(await db.Notifications.OrderByDescending(n => n.CreatedAt).ToListAsync()));

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, NotificationDbContext db) =>
            Results.Ok(await db.Notifications
                .Where(n => n.TenantId == tenantId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync()));

        group.MapPost("/", async (CreateNotificationRequest request, NotificationDbContext db) =>
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                RecipientEmail = request.RecipientEmail,
                Subject = request.Subject,
                Body = request.Body,
                Type = request.Type,
                Status = NotificationStatus.Pending
            };
            db.Notifications.Add(notification);
            await db.SaveChangesAsync();
            return Results.Created($"/api/notifications/{notification.Id}", notification);
        });

        group.MapPost("/{id:guid}/send", async (Guid id, NotificationDbContext db) =>
        {
            var notification = await db.Notifications.FindAsync(id);
            if (notification is null) return Results.NotFound();

            // Simulate sending notification
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(notification);
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a notification.
/// </summary>
public record CreateNotificationRequest(Guid TenantId, string RecipientEmail, string Subject, string Body, NotificationType Type);
