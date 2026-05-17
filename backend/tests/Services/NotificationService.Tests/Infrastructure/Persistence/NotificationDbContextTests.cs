using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using Xunit;

namespace NotificationService.Tests.Infrastructure.Persistence;

public class NotificationDbContextTests
{
    private static DbContextOptions<NotificationDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        using var context = new NotificationDbContext(CreateOptions(Guid.NewGuid().ToString()));
        Assert.NotNull(context);
    }

    [Fact]
    public void DbSets_AreAccessible()
    {
        using var context = new NotificationDbContext(CreateOptions(Guid.NewGuid().ToString()));
        Assert.NotNull(context.Notifications);
        Assert.NotNull(context.Templates);
    }

    [Fact]
    public void SaveChanges_PersistsNotification()
    {
        var options = CreateOptions(Guid.NewGuid().ToString());
        var id = Guid.NewGuid();

        using (var context = new NotificationDbContext(options))
        {
            context.Notifications.Add(new Notification
            {
                Id = id,
                TenantId = Guid.NewGuid(),
                RecipientEmail = "test@example.com",
                Subject = "Test",
                Body = "Body"
            });
            context.SaveChanges();
        }

        using (var context = new NotificationDbContext(options))
        {
            Assert.NotNull(context.Notifications.Find(id));
        }
    }
}
