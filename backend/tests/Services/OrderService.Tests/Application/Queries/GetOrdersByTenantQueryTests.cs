using Microsoft.EntityFrameworkCore;
using OrderService.Application.Queries;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using Xunit;

namespace OrderService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetOrdersByTenantQueryHandler"/>.
/// </summary>
public class GetOrdersByTenantQueryTests
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
            Subtotal = 100.00m,
            ShippingCost = 10.00m,
            TaxAmount = 8.00m,
            Total = 118.00m,
            Currency = "USD",
            Items =
            [
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductVariantId = Guid.NewGuid(),
                    ProductName = "Gadget",
                    Sku = "G-001",
                    UnitPrice = 100.00m,
                    Quantity = 1,
                    LineTotal = 100.00m
                }
            ]
        };

        db.Orders.Add(order);
        db.SaveChanges();
        return order;
    }

    [Fact]
    public async Task Handle_ReturnsOnlyOrdersForSpecifiedTenant()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using var db = CreateContext();
        var orderA = SeedOrder(db, tenantA, "ORD-A-001");
        SeedOrder(db, tenantB, "ORD-B-001");

        var handler = new GetOrdersByTenantQueryHandler(db);
        var query = new GetOrdersByTenantQuery(tenantA);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(orderA.Id, result[0].Id);
        Assert.NotEmpty(result[0].Items);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenTenantHasNoOrders()
    {
        // Arrange
        await using var db = CreateContext();
        SeedOrder(db, Guid.NewGuid(), "ORD-001");

        var handler = new GetOrdersByTenantQueryHandler(db);
        var query = new GetOrdersByTenantQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
