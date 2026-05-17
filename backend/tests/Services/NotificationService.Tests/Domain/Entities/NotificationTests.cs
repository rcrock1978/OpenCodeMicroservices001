using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using Xunit;

namespace NotificationService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Notification"/> entity.
/// </summary>
public class NotificationTests
{
    #region Creation & Properties

    [Fact]
    public void Notification_Created_WithRequiredProperties_ShouldSucceed()
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            RecipientEmail = "test@example.com",
            Subject = "Welcome",
            Body = "Welcome to our platform!"
        };

        Assert.Equal("test@example.com", notification.RecipientEmail);
        Assert.Equal("Welcome", notification.Subject);
        Assert.Equal("Welcome to our platform!", notification.Body);
    }

    [Fact]
    public void Notification_DefaultValues_ShouldBeSet()
    {
        var notification = new Notification
        {
            RecipientEmail = "test@example.com",
            Subject = "Test",
            Body = "Body"
        };

        Assert.Equal(NotificationType.Email, notification.Type);
        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.Null(notification.SentAt);
        Assert.Null(notification.ErrorMessage);
    }

    [Fact]
    public void Notification_Status_CanBeChanged()
    {
        var notification = new Notification
        {
            RecipientEmail = "test@example.com",
            Subject = "Test",
            Body = "Body",
            Status = NotificationStatus.Sent,
            SentAt = DateTime.UtcNow
        };

        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.NotNull(notification.SentAt);
    }

    #endregion

    #region Configuration

    [Fact]
    public void NotificationConfiguration_RecipientEmail_HasMaxLength256AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestNotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestNotificationDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Notification));

        Assert.NotNull(entityType);
        var prop = entityType.FindProperty("RecipientEmail");
        Assert.NotNull(prop);
        Assert.Equal(256, prop.GetMaxLength());
        Assert.False(prop.IsNullable);
    }

    [Fact]
    public void NotificationConfiguration_Subject_HasMaxLength500AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestNotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestNotificationDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Notification));

        Assert.NotNull(entityType);
        var prop = entityType.FindProperty("Subject");
        Assert.NotNull(prop);
        Assert.Equal(500, prop.GetMaxLength());
        Assert.False(prop.IsNullable);
    }

    #endregion

    #region Enum Tests

    [Fact]
    public void NotificationType_HasFourValues()
    {
        Assert.Equal(4, Enum.GetValues<NotificationType>().Length);
    }

    [Fact]
    public void NotificationStatus_HasFourValues()
    {
        Assert.Equal(4, Enum.GetValues<NotificationStatus>().Length);
    }

    #endregion

    #region Integration

    [Fact]
    public void Notification_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestNotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var id = Guid.NewGuid();
        using (var context = new TestNotificationDbContext(options))
        {
            context.Notifications.Add(new Notification
            {
                Id = id,
                TenantId = Guid.NewGuid(),
                RecipientEmail = "persisted@example.com",
                Subject = "Persisted",
                Body = "Persisted body",
                Type = NotificationType.Sms,
                Status = NotificationStatus.Sent
            });
            context.SaveChanges();
        }

        using (var context = new TestNotificationDbContext(options))
        {
            var n = context.Notifications.Find(id);
            Assert.NotNull(n);
            Assert.Equal(NotificationType.Sms, n.Type);
            Assert.Equal(NotificationStatus.Sent, n.Status);
        }
    }

    #endregion
}

/// <summary>
/// In-memory test DbContext for verifying EF Core configurations.
/// </summary>
public class TestNotificationDbContext : DbContext
{
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Template> Templates { get; set; } = null!;

    public TestNotificationDbContext(DbContextOptions<TestNotificationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new TemplateConfiguration());
    }
}
