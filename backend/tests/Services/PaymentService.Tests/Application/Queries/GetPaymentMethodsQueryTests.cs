using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Queries;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using Xunit;

namespace PaymentService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetPaymentMethodsQueryHandler"/>.
/// </summary>
public class GetPaymentMethodsQueryTests
{
    private static PaymentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentDbContext(options);
    }

    [Fact]
    public async Task Handle_WithSeededData_ReturnsAllPaymentMethods()
    {
        // Arrange
        using var context = CreateContext();
        var methods = new[]
        {
            new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Type = PaymentMethodType.Card,
                LastFour = "4242",
                Brand = "Visa"
            },
            new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Type = PaymentMethodType.Wallet,
                Brand = "PayPal"
            }
        };
        context.PaymentMethods.AddRange(methods);
        await context.SaveChangesAsync();

        var handler = new GetPaymentMethodsQueryHandler(context);
        var query = new GetPaymentMethodsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.LastFour == "4242");
        Assert.Contains(result, m => m.Brand == "PayPal");
    }

    [Fact]
    public async Task Handle_WithNoData_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var handler = new GetPaymentMethodsQueryHandler(context);
        var query = new GetPaymentMethodsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
