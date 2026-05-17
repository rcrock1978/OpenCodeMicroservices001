using MediatR;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Commands;

/// <summary>
/// Command to mark a notification as sent.
/// </summary>
public record SendNotificationCommand(Guid Id) : IRequest<Notification?>;

/// <summary>
/// Handles the <see cref="SendNotificationCommand"/>.
/// </summary>
public class SendNotificationHandler(NotificationDbContext db) : IRequestHandler<SendNotificationCommand, Notification?>
{
    /// <inheritdoc />
    public async Task<Notification?> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await db.Notifications.FindAsync(new object[] { request.Id }, cancellationToken);
        if (notification is null)
        {
            return null;
        }

        notification.Status = NotificationStatus.Sent;
        notification.SentAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return notification;
    }
}
