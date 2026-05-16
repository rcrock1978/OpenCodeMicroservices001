using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using SaaSCommon.Messaging.IntegrationEvents;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// Consumes order cancelled events and creates notification records.
/// </summary>
public class OrderCancelledNotificationConsumer : IConsumer<OrderCancelledIntegrationEvent>
{
    private readonly NotificationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderCancelledNotificationConsumer"/> class.
    /// </summary>
    public OrderCancelledNotificationConsumer(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context)
    {
        var evt = context.Message;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = evt.TenantId,
            RecipientEmail = "customer@example.com",
            Subject = $"Order {evt.OrderId} Cancelled",
            Body = $"Your order has been cancelled. Reason: {evt.Reason}",
            Type = NotificationType.Email,
            Status = NotificationStatus.Pending
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        await context.Publish(new NotificationSentIntegrationEvent
        {
            NotificationId = notification.Id,
            TenantId = evt.TenantId,
            Recipient = notification.RecipientEmail,
            Channel = "Email",
            TemplateKey = "order-cancelled"
        });
    }
}
