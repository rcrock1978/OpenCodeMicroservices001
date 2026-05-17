using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Commands;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using Xunit;

namespace PaymentService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="CreatePaymentMethodCommandHandler"/>.
/// </summary>
public class CreatePaymentMethodCommandTests
{
    private static PaymentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesPaymentMethod()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new CreatePaymentMethodCommandHandler(context);
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var command = new CreatePaymentMethodCommand(
            TenantId: tenantId,
            CustomerId: customerId,
            Type: PaymentMethodType.Card,
            LastFour: "4242",
            Brand: "Visa",
            ExpMonth: 12,
            ExpYear: 2027,
            IsDefault: true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(PaymentMethodType.Card, result.Type);
        Assert.Equal("4242", result.LastFour);
        Assert.Equal("Visa", result.Brand);
        Assert.Equal(12, result.ExpMonth);
        Assert.Equal(2027, result.ExpYear);
        Assert.True(result.IsDefault);

        var persisted = await context.PaymentMethods.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal("Visa", persisted.Brand);
    }

    [Fact]
    public async Task Handle_MinimalCommand_CreatesPaymentMethodWithDefaults()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new CreatePaymentMethodCommandHandler(context);
        var command = new CreatePaymentMethodCommand(
            TenantId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            Type: PaymentMethodType.BankTransfer,
            LastFour: null,
            Brand: null,
            ExpMonth: null,
            ExpYear: null,
            IsDefault: false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PaymentMethodType.BankTransfer, result.Type);
        Assert.Null(result.LastFour);
        Assert.Null(result.Brand);
        Assert.Null(result.ExpMonth);
        Assert.Null(result.ExpYear);
        Assert.False(result.IsDefault);

        var persisted = await context.PaymentMethods.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(PaymentMethodType.BankTransfer, persisted.Type);
    }
}
