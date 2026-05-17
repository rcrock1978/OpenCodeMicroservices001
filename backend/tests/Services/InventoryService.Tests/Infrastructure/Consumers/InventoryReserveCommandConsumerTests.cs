using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Consumers;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging;
using SaaSCommon.Messaging.IntegrationEvents;
using Xunit;

namespace InventoryService.Tests.Infrastructure.Consumers;

/// <summary>
/// Unit tests for the <see cref="InventoryReserveCommandConsumer"/>.
/// </summary>
public class InventoryReserveCommandConsumerTests
{
    private static InventoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InventoryDbContext(options);
    }

    [Fact]
    public async Task Consume_WithAvailableStock_ShouldReserveAndCreateMovement()
    {
        var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var productVariantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        db.StockItems.Add(new StockItem
        {
            Id = itemId,
            TenantId = tenantId,
            ProductVariantId = productVariantId,
            Sku = "RESERVE-001",
            QuantityAvailable = 100,
            QuantityReserved = 0
        });
        await db.SaveChangesAsync();

        var consumer = new InventoryReserveCommandConsumer(db);
        var command = new InventoryReserveCommand
        {
            OrderId = orderId,
            TenantId = tenantId,
            Items = new Dictionary<Guid, int> { { productVariantId, 10 } }
        };

        var fakeContext = new FakeConsumeContext<InventoryReserveCommand>(command);
        await consumer.Consume(fakeContext);

        var item = await db.StockItems.FindAsync(itemId);
        var reservation = await db.StockReservations
            .FirstOrDefaultAsync(r => r.OrderId == orderId && r.TenantId == tenantId);
        var movement = await db.StockMovements
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Type == StockMovementType.Reservation);

        Assert.NotNull(item);
        Assert.Equal(10, item.QuantityReserved);
        Assert.NotNull(reservation);
        Assert.Equal(10, reservation.Quantity);
        Assert.Equal(ReservationStatus.Reserved, reservation.Status);
        Assert.NotNull(movement);
        Assert.Equal(10, movement.Quantity);
    }

    [Fact]
    public async Task Consume_WithInsufficientStock_ShouldNotReserve()
    {
        var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var productVariantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        db.StockItems.Add(new StockItem
        {
            Id = itemId,
            TenantId = tenantId,
            ProductVariantId = productVariantId,
            Sku = "LOW-001",
            QuantityAvailable = 5,
            QuantityReserved = 0
        });
        await db.SaveChangesAsync();

        var consumer = new InventoryReserveCommandConsumer(db);
        var command = new InventoryReserveCommand
        {
            OrderId = orderId,
            TenantId = tenantId,
            Items = new Dictionary<Guid, int> { { productVariantId, 10 } }
        };

        var fakeContext = new FakeConsumeContext<InventoryReserveCommand>(command);
        await consumer.Consume(fakeContext);

        var item = await db.StockItems.FindAsync(itemId);
        Assert.Equal(0, item!.QuantityReserved);
        Assert.Empty(db.StockReservations);
    }

    [Fact]
    public async Task Consume_WithMissingStockItem_ShouldNotReserve()
    {
        var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var productVariantId = Guid.NewGuid();

        var consumer = new InventoryReserveCommandConsumer(db);
        var command = new InventoryReserveCommand
        {
            OrderId = orderId,
            TenantId = tenantId,
            Items = new Dictionary<Guid, int> { { productVariantId, 5 } }
        };

        var fakeContext = new FakeConsumeContext<InventoryReserveCommand>(command);
        await consumer.Consume(fakeContext);

        Assert.Empty(db.StockReservations);
        Assert.Empty(db.StockMovements);
    }

    [Fact]
    public async Task Consume_MultipleItems_ShouldReserveAll()
    {
        var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var variantId1 = Guid.NewGuid();
        var variantId2 = Guid.NewGuid();
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();

        db.StockItems.AddRange(
            new StockItem { Id = itemId1, TenantId = tenantId, ProductVariantId = variantId1, Sku = "MULTI-1", QuantityAvailable = 50, QuantityReserved = 0 },
            new StockItem { Id = itemId2, TenantId = tenantId, ProductVariantId = variantId2, Sku = "MULTI-2", QuantityAvailable = 30, QuantityReserved = 0 }
        );
        await db.SaveChangesAsync();

        var consumer = new InventoryReserveCommandConsumer(db);
        var command = new InventoryReserveCommand
        {
            OrderId = orderId,
            TenantId = tenantId,
            Items = new Dictionary<Guid, int> { { variantId1, 5 }, { variantId2, 3 } }
        };

        var fakeContext = new FakeConsumeContext<InventoryReserveCommand>(command);
        await consumer.Consume(fakeContext);

        var item1 = await db.StockItems.FindAsync(itemId1);
        var item2 = await db.StockItems.FindAsync(itemId2);

        Assert.Equal(5, item1!.QuantityReserved);
        Assert.Equal(3, item2!.QuantityReserved);
        Assert.Equal(2, await db.StockReservations.CountAsync());
        Assert.Equal(2, await db.StockMovements.CountAsync());
    }
}
