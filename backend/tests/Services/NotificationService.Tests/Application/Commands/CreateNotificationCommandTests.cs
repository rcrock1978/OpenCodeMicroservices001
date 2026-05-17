using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Commands;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using Xunit;

namespace NotificationService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="CreateNotificationHandler"/>.
/// </summary>
public class CreateNotificationCommandTests
{
    private static NotificationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NotificationDbContext(options);
    }

    [Fact]
    public async Task Handle_CreatesNotification_WithPendingStatus()
    {
        using var context = CreateContext();
        var handler = new CreateNotificationHandler(context);
        var tenantId = Guid.NewGuid();

        var command = new CreateNotificationCommand(
            tenantId,
            "user@example.com",
            "Test Subject",
            "Test Body",
            NotificationType.Email);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal("user@example.com", result.RecipientEmail);
        Assert.Equal("Test Subject", result.Subject);
        Assert.Equal("Test Body", result.Body);
        Assert.Equal(NotificationType.Email, result.Type);
        Assert.Equal(NotificationStatus.Pending, result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Handle_PersistsNotification_ToDatabase()
    {
        using var context = CreateContext();
        var handler = new CreateNotificationHandler(context);

        var command = new CreateNotificationCommand(
            Guid.NewGuid(),
            "user@example.com",
            "Subject",
            "Body",
            NotificationType.Sms);

        var result = await handler.Handle(command, CancellationToken.None);

        var persisted = await context.Notifications.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(NotificationType.Sms, persisted.Type);
    }
}
