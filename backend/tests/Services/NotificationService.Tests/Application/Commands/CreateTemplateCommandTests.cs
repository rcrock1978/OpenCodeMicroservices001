using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Commands;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using Xunit;

namespace NotificationService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTemplateHandler"/>.
/// </summary>
public class CreateTemplateCommandTests
{
    private static NotificationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NotificationDbContext(options);
    }

    [Fact]
    public async Task Handle_CreatesTemplate_WithProvidedValues()
    {
        using var context = CreateContext();
        var handler = new CreateTemplateHandler(context);
        var tenantId = Guid.NewGuid();

        var command = new CreateTemplateCommand(
            tenantId,
            "welcome",
            "Welcome!",
            "<p>Welcome</p>",
            "Welcome",
            NotificationChannel.Email);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal("welcome", result.Key);
        Assert.Equal("Welcome!", result.Subject);
        Assert.Equal("<p>Welcome</p>", result.BodyHtml);
        Assert.Equal("Welcome", result.BodyText);
        Assert.Equal(NotificationChannel.Email, result.Channel);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Handle_PersistsTemplate_ToDatabase()
    {
        using var context = CreateContext();
        var handler = new CreateTemplateHandler(context);

        var command = new CreateTemplateCommand(
            Guid.NewGuid(),
            "order-confirmation",
            "Confirmed",
            null,
            null,
            NotificationChannel.Push);

        var result = await handler.Handle(command, CancellationToken.None);

        var persisted = await context.Templates.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal("order-confirmation", persisted.Key);
        Assert.Equal(NotificationChannel.Push, persisted.Channel);
    }
}
