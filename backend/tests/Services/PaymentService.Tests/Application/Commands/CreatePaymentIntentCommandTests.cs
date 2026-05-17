using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Commands;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using Xunit;

namespace PaymentService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="CreatePaymentIntentCommandHandler"/>.
/// </summary>
public class CreatePaymentIntentCommandTests
{
    private static PaymentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentDbContext(options);
    }

    [Fact]
    public async Task Handle_PositiveAmountAndNoTestFailure_CreatesSucceededIntent()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new CreatePaymentIntentCommandHandler(context);
        var command = new CreatePaymentIntentCommand(
            TenantId: Guid.NewGuid(),
            OrderId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            Amount: 99.99m,
            Currency: "USD",
            IdempotencyKey: "idemp_1",
            PaymentMethod: "card",
            TestFailure: false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(PaymentStatus.Succeeded, result.Status);
        Assert.Null(result.FailureReason);
        Assert.NotNull(result.CapturedAt);

        var persisted = await context.PaymentIntents.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(PaymentStatus.Succeeded, persisted.Status);
    }

    [Fact]
    public async Task Handle_ZeroAmount_CreatesFailedIntent()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new CreatePaymentIntentCommandHandler(context);
        var command = new CreatePaymentIntentCommand(
            TenantId: Guid.NewGuid(),
            OrderId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            Amount: 0m,
            Currency: "USD",
            IdempotencyKey: "idemp_zero",
            PaymentMethod: "card",
            TestFailure: false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Failed, result.Status);
        Assert.Equal("Test failure or zero amount", result.FailureReason);
        Assert.Null(result.CapturedAt);

        var persisted = await context.PaymentIntents.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(PaymentStatus.Failed, persisted.Status);
    }

    [Fact]
    public async Task Handle_NegativeAmount_CreatesFailedIntent()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new CreatePaymentIntentCommandHandler(context);
        var command = new CreatePaymentIntentCommand(
            TenantId: Guid.NewGuid(),
            OrderId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            Amount: -10m,
            Currency: "USD",
            IdempotencyKey: "idemp_neg",
            PaymentMethod: "card",
            TestFailure: false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Failed, result.Status);
        Assert.Equal("Test failure or zero amount", result.FailureReason);
        Assert.Null(result.CapturedAt);
    }

    [Fact]
    public async Task Handle_TestFailureTrue_CreatesFailedIntent()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new CreatePaymentIntentCommandHandler(context);
        var command = new CreatePaymentIntentCommand(
            TenantId: Guid.NewGuid(),
            OrderId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            Amount: 50m,
            Currency: "EUR",
            IdempotencyKey: "idemp_fail",
            PaymentMethod: "card",
            TestFailure: true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Failed, result.Status);
        Assert.Equal("Test failure or zero amount", result.FailureReason);
        Assert.Null(result.CapturedAt);

        var persisted = await context.PaymentIntents.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(PaymentStatus.Failed, persisted.Status);
    }
}
