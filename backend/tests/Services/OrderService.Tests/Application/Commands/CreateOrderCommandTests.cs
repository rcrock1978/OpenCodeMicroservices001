using Microsoft.EntityFrameworkCore;
using OrderService.Application.Commands;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using OrderService.Tests.Fakes;
using SaaSCommon.Messaging.IntegrationEvents;
using Xunit;

namespace OrderService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="CreateOrderCommandHandler"/>.
/// </summary>
public class CreateOrderCommandTests
{
    private static OrderDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderDbContext(options);
    }

    [Fact]
    public async Task Handle_CreatesOrder_WithCorrectTotal()
    {
        // Arrange
        await using var db = CreateContext();
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CreateOrderCommandHandler(db, fakePublisher);

        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var items = new List<CreateOrderItemDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Product A", "A-001", 25.00m, 2),
            new(Guid.NewGuid(), Guid.NewGuid(), "Product B", "B-001", 50.00m, 1)
        };

        var command = new CreateOrderCommand(
            tenantId,
            customerId,
            items,
            10.00m,
            8.00m,
            "USD",
            "123 Main St");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Equal(100.00m, result.Subtotal); // 25*2 + 50*1
        Assert.Equal(10.00m, result.ShippingCost);
        Assert.Equal(8.00m, result.TaxAmount);
        Assert.Equal(118.00m, result.Total); // 100 + 10 + 8
        Assert.Equal("USD", result.Currency);
        Assert.Equal("123 Main St", result.ShippingAddress);
        Assert.NotNull(result.OrderNumber);
        Assert.StartsWith("ORD-", result.OrderNumber);

        // Verify items
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, i => i.Sku == "A-001" && i.LineTotal == 50.00m);
        Assert.Contains(result.Items, i => i.Sku == "B-001" && i.LineTotal == 50.00m);

        // Verify persistence
        var persistedOrder = await db.Orders.FindAsync(result.Id);
        Assert.NotNull(persistedOrder);
        Assert.Equal(118.00m, persistedOrder.Total);
    }

    [Fact]
    public async Task Handle_PublishesOrderPlacedIntegrationEvent()
    {
        // Arrange
        await using var db = CreateContext();
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CreateOrderCommandHandler(db, fakePublisher);

        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var items = new List<CreateOrderItemDto>
        {
            new(productId, variantId, "Widget", "W-001", 10.00m, 3)
        };

        var command = new CreateOrderCommand(
            tenantId,
            customerId,
            items,
            5.00m,
            2.00m,
            "USD",
            null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var publishedEvent = fakePublisher.GetPublishedMessage<OrderPlacedIntegrationEvent>();
        Assert.NotNull(publishedEvent);
        Assert.Equal(result.Id, publishedEvent.OrderId);
        Assert.Equal(tenantId, publishedEvent.TenantId);
        Assert.Equal(customerId, publishedEvent.CustomerId);
        Assert.Equal(37.00m, publishedEvent.TotalAmount); // 30 + 5 + 2
        Assert.Single(publishedEvent.Items);
        Assert.Equal(productId, publishedEvent.Items[0].ProductId);
        Assert.Equal(variantId, publishedEvent.Items[0].ProductVariantId);
        Assert.Equal("Widget", publishedEvent.Items[0].ProductName);
        Assert.Equal("W-001", publishedEvent.Items[0].Sku);
        Assert.Equal(10.00m, publishedEvent.Items[0].UnitPrice);
        Assert.Equal(3, publishedEvent.Items[0].Quantity);
    }

    [Fact]
    public async Task Handle_CreatesOrder_WithNoItems()
    {
        // Arrange
        await using var db = CreateContext();
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CreateOrderCommandHandler(db, fakePublisher);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new List<CreateOrderItemDto>(),
            5.00m,
            0.00m,
            "EUR",
            null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.00m, result.Subtotal);
        Assert.Equal(5.00m, result.Total);
        Assert.Empty(result.Items);

        var publishedEvent = fakePublisher.GetPublishedMessage<OrderPlacedIntegrationEvent>();
        Assert.NotNull(publishedEvent);
        Assert.Equal(5.00m, publishedEvent.TotalAmount);
        Assert.Empty(publishedEvent.Items);
    }
}
