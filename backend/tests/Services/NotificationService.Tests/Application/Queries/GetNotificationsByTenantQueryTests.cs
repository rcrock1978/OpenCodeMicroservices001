using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Queries;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using Xunit;

namespace NotificationService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetNotificationsByTenantHandler"/>.
/// </summary>
public class GetNotificationsByTenantQueryTests
{
    private static NotificationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NotificationDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyNotificationsForSpecifiedTenant()
    {
        using var context = CreateContext();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var n1 = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantA,
            RecipientEmail = "a@example.com",
            Subject = "Tenant A - 1",
            Body = "Body",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        var n2 = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantB,
            RecipientEmail = "b@example.com",
            Subject = "Tenant B - 1",
            Body = "Body",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        var n3 = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantA,
            RecipientEmail = "c@example.com",
            Subject = "Tenant A - 2",
            Body = "Body",
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.AddRange(n1, n2, n3);
        await context.SaveChangesAsync();

        var handler = new GetNotificationsByTenantHandler(context);
        var result = await handler.Handle(new GetNotificationsByTenantQuery(tenantA), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, n => Assert.Equal(tenantA, n.TenantId));
        Assert.Equal(n3.Id, result[0].Id);
        Assert.Equal(n1.Id, result[1].Id);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenTenantHasNoNotifications()
    {
        using var context = CreateContext();
        var tenant = Guid.NewGuid();

        context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            RecipientEmail = "other@example.com",
            Subject = "Other",
            Body = "Body"
        });
        await context.SaveChangesAsync();

        var handler = new GetNotificationsByTenantHandler(context);
        var result = await handler.Handle(new GetNotificationsByTenantQuery(tenant), CancellationToken.None);

        Assert.Empty(result);
    }
}
