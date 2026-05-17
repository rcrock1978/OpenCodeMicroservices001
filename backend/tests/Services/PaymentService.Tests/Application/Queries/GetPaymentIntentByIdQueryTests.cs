using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Queries;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using Xunit;

namespace PaymentService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetPaymentIntentByIdQueryHandler"/>.
/// </summary>
public class GetPaymentIntentByIdQueryTests
{
    private static PaymentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingId_ReturnsPaymentIntent()
    {
        // Arrange
        using var context = CreateContext();
        var id = Guid.NewGuid();
        var intent = new PaymentIntent
        {
            Id = id,
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Amount = 150m,
            Currency = "USD",
            IdempotencyKey = "key_existing"
        };
        context.PaymentIntents.Add(intent);
        await context.SaveChangesAsync();

        var handler = new GetPaymentIntentByIdQueryHandler(context);
        var query = new GetPaymentIntentByIdQuery(id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public async Task Handle_NonExistingId_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new GetPaymentIntentByIdQueryHandler(context);
        var query = new GetPaymentIntentByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
