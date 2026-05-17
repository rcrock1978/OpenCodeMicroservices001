using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Queries;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using Xunit;

namespace NotificationService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetTemplatesHandler"/>.
/// </summary>
public class GetTemplatesQueryTests
{
    private static NotificationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NotificationDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsAllTemplates()
    {
        using var context = CreateContext();
        var t1 = new Template
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Key = "welcome",
            Subject = "Welcome"
        };
        var t2 = new Template
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Key = "order-confirmation",
            Subject = "Order Confirmed"
        };

        context.Templates.AddRange(t1, t2);
        await context.SaveChangesAsync();

        var handler = new GetTemplatesHandler(context);
        var result = await handler.Handle(new GetTemplatesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoTemplates()
    {
        using var context = CreateContext();
        var handler = new GetTemplatesHandler(context);
        var result = await handler.Handle(new GetTemplatesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
