using Microsoft.EntityFrameworkCore;
using OrderService.Application.Commands;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using OrderService.Tests.Fakes;
using SaaSCommon.Messaging.IntegrationEvents;
using Xunit;

namespace OrderService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="CancelOrderCommandHandler"/>.
/// </summary>
public class CancelOrderCommandTests
{
    private static OrderDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderDbContext(options);
    }

    private static Order SeedOrder(OrderDbContext db, OrderStatus status, string orderNumber = "ORD-001")
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Status = status,
            Subtotal = 50.00m,
            ShippingCost = 5.00m,
            TaxAmount = 4.00m,
            Total = 59.00m,
            Currency = "USD"
        };

        db.Orders.Add(order);
        db.SaveChanges();
        return order;
    }

    [Fact]
    public async Task Handle_SetsStatusToCancelled_WhenOrderIsPending()
    {
        // Arrange
        await using var db = CreateContext();
        var order = SeedOrder(db, OrderStatus.Pending);
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CancelOrderCommandHandler(db, fakePublisher);
        var command = new CancelOrderCommand(order.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Order);
        Assert.Equal(order.Id, result.Order.Id);
        Assert.Equal(OrderStatus.Cancelled, result.Order.Status);
        Assert.Null(result.Error);

        var persistedOrder = await db.Orders.FindAsync(order.Id);
        Assert.NotNull(persistedOrder);
        Assert.Equal(OrderStatus.Cancelled, persistedOrder.Status);

        var publishedEvent = fakePublisher.GetPublishedMessage<OrderCancelledIntegrationEvent>();
        Assert.NotNull(publishedEvent);
        Assert.Equal(order.Id, publishedEvent.OrderId);
        Assert.Equal(order.TenantId, publishedEvent.TenantId);
        Assert.Equal("Customer requested cancellation", publishedEvent.Reason);
    }

    [Fact]
    public async Task Handle_SetsStatusToCancelled_WhenOrderIsPaid()
    {
        // Arrange
        await using var db = CreateContext();
        var order = SeedOrder(db, OrderStatus.Paid);
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CancelOrderCommandHandler(db, fakePublisher);
        var command = new CancelOrderCommand(order.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Cancelled, result.Order!.Status);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        await using var db = CreateContext();
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CancelOrderCommandHandler(db, fakePublisher);
        var command = new CancelOrderCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.Order);
        Assert.Equal("Order not found", result.Error);
        Assert.Empty(fakePublisher.PublishedMessages);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    public async Task Handle_ReturnsError_WhenOrderIsShippedOrDelivered(OrderStatus status)
    {
        // Arrange
        await using var db = CreateContext();
        var order = SeedOrder(db, status);
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CancelOrderCommandHandler(db, fakePublisher);
        var command = new CancelOrderCommand(order.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Order);
        Assert.Equal(order.Id, result.Order.Id);
        Assert.Equal(status, result.Order.Status);
        Assert.Equal("Cannot cancel shipped or delivered order", result.Error);
        Assert.Empty(fakePublisher.PublishedMessages);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenOrderIsInventoryReserved()
    {
        // Arrange
        await using var db = CreateContext();
        var order = SeedOrder(db, OrderStatus.InventoryReserved);
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CancelOrderCommandHandler(db, fakePublisher);
        var command = new CancelOrderCommand(order.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Cancelled, result.Order!.Status);
    }
}
