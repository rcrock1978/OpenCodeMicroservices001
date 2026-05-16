using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using SaaSCommon.Messaging.IntegrationEvents;

namespace NotificationService.Infrastructure.Consumers;

/// <summary>
/// Consumes customer created events and creates welcome notifications.
/// </summary>
public class CustomerCreatedNotificationConsumer : IConsumer<CustomerCreatedIntegrationEvent>
{
    private readonly NotificationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerCreatedNotificationConsumer"/> class.
    /// </summary>
    public CustomerCreatedNotificationConsumer(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<CustomerCreatedIntegrationEvent> context)
    {
        var evt = context.Message;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = evt.TenantId,
            RecipientEmail = evt.Email,
            Subject = "Welcome to our store",
            Body = $"Hi {evt.FullName}, welcome to our platform!",
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
            TemplateKey = "welcome"
        });
    }
}
