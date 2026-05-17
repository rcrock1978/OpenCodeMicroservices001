using Microsoft.EntityFrameworkCore;
using OrderService.Application.Queries;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using Xunit;

namespace OrderService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetOrdersQueryHandler"/>.
/// </summary>
public class GetOrdersQueryTests
{
    private static OrderDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderDbContext(options);
    }

    private static Order SeedOrder(OrderDbContext db, Guid tenantId, string orderNumber, OrderStatus status = OrderStatus.Pending)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Status = status,
            Subtotal = 50.00m,
            ShippingCost = 5.00m,
            TaxAmount = 4.00m,
            Total = 59.00m,
            Currency = "USD",
            Items =
            [
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductVariantId = Guid.NewGuid(),
                    ProductName = "Widget",
                    Sku = "W-001",
                    UnitPrice = 25.00m,
                    Quantity = 2,
                    LineTotal = 50.00m
                }
            ]
        };

        db.Orders.Add(order);
        db.SaveChanges();
        return order;
    }

    [Fact]
    public async Task Handle_ReturnsAllOrders_WithItems()
    {
        // Arrange
        await using var db = CreateContext();
        var order1 = SeedOrder(db, Guid.NewGuid(), "ORD-001");
        var order2 = SeedOrder(db, Guid.NewGuid(), "ORD-002");

        var handler = new GetOrdersQueryHandler(db);
        var query = new GetOrdersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.NotEmpty(o.Items));
        Assert.Contains(result, o => o.Id == order1.Id);
        Assert.Contains(result, o => o.Id == order2.Id);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoOrdersExist()
    {
        // Arrange
        await using var db = CreateContext();
        var handler = new GetOrdersQueryHandler(db);
        var query = new GetOrdersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
