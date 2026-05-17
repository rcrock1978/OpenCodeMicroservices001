using Microsoft.EntityFrameworkCore;
using OrderService.Application.Queries;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using Xunit;

namespace OrderService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetOrderByIdQueryHandler"/>.
/// </summary>
public class GetOrderByIdQueryTests
{
    private static OrderDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderDbContext(options);
    }

    private static Order SeedOrder(OrderDbContext db, Guid tenantId, string orderNumber)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Subtotal = 75.00m,
            ShippingCost = 5.00m,
            TaxAmount = 6.00m,
            Total = 86.00m,
            Currency = "USD",
            Items =
            [
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductVariantId = Guid.NewGuid(),
                    ProductName = "Thingamajig",
                    Sku = "T-001",
                    UnitPrice = 75.00m,
                    Quantity = 1,
                    LineTotal = 75.00m
                }
            ]
        };

        db.Orders.Add(order);
        db.SaveChanges();
        return order;
    }

    [Fact]
    public async Task Handle_ReturnsOrderWithItems_WhenOrderExists()
    {
        // Arrange
        await using var db = CreateContext();
        var seededOrder = SeedOrder(db, Guid.NewGuid(), "ORD-001");

        var handler = new GetOrderByIdQueryHandler(db);
        var query = new GetOrderByIdQuery(seededOrder.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(seededOrder.Id, result.Id);
        Assert.Equal(seededOrder.OrderNumber, result.OrderNumber);
        Assert.Single(result.Items);
        Assert.Equal("Thingamajig", result.Items.First().ProductName);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenOrderDoesNotExist()
    {
        // Arrange
        await using var db = CreateContext();
        var handler = new GetOrderByIdQueryHandler(db);
        var query = new GetOrderByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
