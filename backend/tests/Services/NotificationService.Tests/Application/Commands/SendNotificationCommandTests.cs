using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Commands;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using Xunit;

namespace NotificationService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="SendNotificationHandler"/>.
/// </summary>
public class SendNotificationCommandTests
{
    private static NotificationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NotificationDbContext(options);
    }

    [Fact]
    public async Task Handle_SetsStatusToSent_And_SentAtToUtcNow()
    {
        using var context = CreateContext();
        var id = Guid.NewGuid();
        var notification = new Notification
        {
            Id = id,
            TenantId = Guid.NewGuid(),
            RecipientEmail = "user@example.com",
            Subject = "Subject",
            Body = "Body",
            Status = NotificationStatus.Pending
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        var before = DateTime.UtcNow;
        var handler = new SendNotificationHandler(context);
        var result = await handler.Handle(new SendNotificationCommand(id), CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.NotNull(result);
        Assert.Equal(NotificationStatus.Sent, result.Status);
        Assert.NotNull(result.SentAt);
        Assert.True(result.SentAt.Value >= before);
        Assert.True(result.SentAt.Value <= after);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenNotificationNotFound()
    {
        using var context = CreateContext();
        var handler = new SendNotificationHandler(context);
        var result = await handler.Handle(new SendNotificationCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
