using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using Xunit;

namespace NotificationService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Template"/> entity.
/// </summary>
public class TemplateTests
{
    [Fact]
    public void Template_Created_WithRequiredProperties_ShouldSucceed()
    {
        var template = new Template
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Key = "order-confirmation",
            Subject = "Your order is confirmed"
        };

        Assert.Equal("order-confirmation", template.Key);
        Assert.Equal("Your order is confirmed", template.Subject);
    }

    [Fact]
    public void Template_DefaultValues_ShouldBeSet()
    {
        var template = new Template
        {
            Key = "welcome",
            Subject = "Welcome"
        };

        Assert.Equal(NotificationChannel.Email, template.Channel);
        Assert.Null(template.BodyHtml);
        Assert.Null(template.BodyText);
    }

    [Fact]
    public void TemplateConfiguration_HasUniqueIndexOnTenantIdAndKey()
    {
        var options = new DbContextOptionsBuilder<TestNotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestNotificationDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Template));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantKeyIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "Key"));

        Assert.NotNull(tenantKeyIndex);
        Assert.True(tenantKeyIndex.IsUnique);
    }

    [Fact]
    public void TemplateConfiguration_Key_HasMaxLength100AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestNotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestNotificationDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Template));

        Assert.NotNull(entityType);
        var prop = entityType.FindProperty("Key");
        Assert.NotNull(prop);
        Assert.Equal(100, prop.GetMaxLength());
        Assert.False(prop.IsNullable);
    }

    [Fact]
    public void TemplateConfiguration_HasIndexOnTenantIdAndKey()
    {
        var options = new DbContextOptionsBuilder<TestNotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestNotificationDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Template));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantKeyIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "Key"));

        Assert.NotNull(tenantKeyIndex);
        Assert.True(tenantKeyIndex.IsUnique);
    }
}
