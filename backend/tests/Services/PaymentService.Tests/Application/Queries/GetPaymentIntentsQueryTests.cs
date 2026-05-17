using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Queries;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using Xunit;

namespace PaymentService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetPaymentIntentsQueryHandler"/>.
/// </summary>
public class GetPaymentIntentsQueryTests
{
    private static PaymentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentDbContext(options);
    }

    [Fact]
    public async Task Handle_WithSeededData_ReturnsAllPaymentIntents()
    {
        // Arrange
        using var context = CreateContext();
        var intents = new[]
        {
            new PaymentIntent
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Amount = 100m,
                Currency = "USD",
                IdempotencyKey = "key_1"
            },
            new PaymentIntent
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Amount = 200m,
                Currency = "EUR",
                IdempotencyKey = "key_2"
            }
        };
        context.PaymentIntents.AddRange(intents);
        await context.SaveChangesAsync();

        var handler = new GetPaymentIntentsQueryHandler(context);
        var query = new GetPaymentIntentsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.IdempotencyKey == "key_1");
        Assert.Contains(result, p => p.IdempotencyKey == "key_2");
    }

    [Fact]
    public async Task Handle_WithNoData_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new GetPaymentIntentsQueryHandler(context);
        var query = new GetPaymentIntentsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
