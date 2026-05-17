using MediatR;
using NotificationService.Application.Commands;
using NotificationService.Application.Queries;
using NotificationService.Domain.Entities;

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

        group.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new GetNotificationsQuery(), cancellationToken)));

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new GetNotificationsByTenantQuery(tenantId), cancellationToken)));

        group.MapPost("/", async (CreateNotificationRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreateNotificationCommand(
                request.TenantId,
                request.RecipientEmail,
                request.Subject,
                request.Body,
                request.Type);
            var notification = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/notifications/{notification.Id}", notification);
        });

        group.MapPost("/{id:guid}/send", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var notification = await mediator.Send(new SendNotificationCommand(id), cancellationToken);
            return notification is null ? Results.NotFound() : Results.Ok(notification);
        });

        group.MapGet("/templates", async (IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new GetTemplatesQuery(), cancellationToken)));

        group.MapPost("/templates", async (CreateTemplateRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreateTemplateCommand(
                request.TenantId,
                request.Key,
                request.Subject,
                request.BodyHtml,
                request.BodyText,
                request.Channel);
            var template = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/notifications/templates/{template.Id}", template);
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a notification.
/// </summary>
public record CreateNotificationRequest(Guid TenantId, string RecipientEmail, string Subject, string Body, NotificationType Type);

/// <summary>
/// Request model for creating a template.
/// </summary>
public record CreateTemplateRequest(Guid TenantId, string Key, string Subject, string? BodyHtml, string? BodyText, NotificationChannel Channel);
