using MediatR;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Commands;

/// <summary>
/// Command to create a new notification.
/// </summary>
public record CreateNotificationCommand(
    Guid TenantId,
    string RecipientEmail,
    string Subject,
    string Body,
    NotificationType Type) : IRequest<Notification>;

/// <summary>
/// Handles the <see cref="CreateNotificationCommand"/>.
/// </summary>
public class CreateNotificationHandler(NotificationDbContext db) : IRequestHandler<CreateNotificationCommand, Notification>
{
    /// <inheritdoc />
    public async Task<Notification> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
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
        await db.SaveChangesAsync(cancellationToken);
        return notification;
    }
}
