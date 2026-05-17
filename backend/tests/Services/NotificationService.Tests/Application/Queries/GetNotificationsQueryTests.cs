using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Queries;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using Xunit;

namespace NotificationService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetNotificationsHandler"/>.
/// </summary>
public class GetNotificationsQueryTests
{
    private static NotificationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NotificationDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsNotifications_OrderedByCreatedAtDescending()
    {
        using var context = CreateContext();
        var now = DateTime.UtcNow;

        var n1 = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            RecipientEmail = "a@example.com",
            Subject = "First",
            Body = "Body",
            CreatedAt = now.AddHours(-2)
        };
        var n2 = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            RecipientEmail = "b@example.com",
            Subject = "Second",
            Body = "Body",
            CreatedAt = now.AddHours(-1)
        };
        var n3 = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            RecipientEmail = "c@example.com",
            Subject = "Third",
            Body = "Body",
            CreatedAt = now
        };

        context.Notifications.AddRange(n1, n2, n3);
        await context.SaveChangesAsync();

        var handler = new GetNotificationsHandler(context);
        var result = await handler.Handle(new GetNotificationsQuery(), CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(n3.Id, result[0].Id);
        Assert.Equal(n2.Id, result[1].Id);
        Assert.Equal(n1.Id, result[2].Id);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoNotifications()
    {
        using var context = CreateContext();
        var handler = new GetNotificationsHandler(context);
        var result = await handler.Handle(new GetNotificationsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
