using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Commands;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using Xunit;

namespace PaymentService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="RefundPaymentIntentCommandHandler"/>.
/// </summary>
public class RefundPaymentIntentCommandTests
{
    private static PaymentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentDbContext(options);
    }

    [Fact]
    public async Task Handle_SucceededIntent_UpdatesStatusToRefunded()
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
            Amount = 100m,
            Currency = "USD",
            IdempotencyKey = "refund_key_1",
            Status = PaymentStatus.Succeeded
        };
        context.PaymentIntents.Add(intent);
        await context.SaveChangesAsync();

        var handler = new RefundPaymentIntentCommandHandler(context);
        var command = new RefundPaymentIntentCommand(id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Refunded, result.Status);

        var persisted = await context.PaymentIntents.FindAsync(id);
        Assert.NotNull(persisted);
        Assert.Equal(PaymentStatus.Refunded, persisted.Status);
    }

    [Fact]
    public async Task Handle_NonExistingId_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new RefundPaymentIntentCommandHandler(context);
        var command = new RefundPaymentIntentCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_PendingIntent_ThrowsInvalidOperationException()
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
            Amount = 100m,
            Currency = "USD",
            IdempotencyKey = "refund_key_pending",
            Status = PaymentStatus.Pending
        };
        context.PaymentIntents.Add(intent);
        await context.SaveChangesAsync();

        var handler = new RefundPaymentIntentCommandHandler(context);
        var command = new RefundPaymentIntentCommand(id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Only succeeded payments can be refunded", ex.Message);
    }

    [Fact]
    public async Task Handle_FailedIntent_ThrowsInvalidOperationException()
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
            Amount = 100m,
            Currency = "USD",
            IdempotencyKey = "refund_key_failed",
            Status = PaymentStatus.Failed
        };
        context.PaymentIntents.Add(intent);
        await context.SaveChangesAsync();

        var handler = new RefundPaymentIntentCommandHandler(context);
        var command = new RefundPaymentIntentCommand(id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Only succeeded payments can be refunded", ex.Message);
    }
}
