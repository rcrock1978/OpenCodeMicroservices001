using InventoryService.Application.Commands;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryService.Tests.Application.Commands;

/// <summary>
/// Unit tests for the <see cref="ReserveStockCommandHandler"/>.
/// </summary>
public class ReserveStockCommandTests
{
    private static DbContextOptions<InventoryDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task Handle_WithSufficientStock_ReservesAndReturnsSuccess()
    {
        var options = CreateInMemoryOptions(nameof(Handle_WithSufficientStock_ReservesAndReturnsSuccess));
        using var context = new InventoryDbContext(options);
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        context.StockItems.Add(new StockItem
        {
            Id = itemId,
            TenantId = tenantId,
            ProductVariantId = Guid.NewGuid(),
            Sku = "RES-001",
            QuantityAvailable = 100,
            QuantityReserved = 10
        });
        await context.SaveChangesAsync();

        var handler = new ReserveStockCommandHandler(context);
        var result = await handler.Handle(
            new ReserveStockCommand(tenantId, "RES-001", orderId, 20),
            CancellationToken.None);

        Assert.Equal(ReserveStockStatus.Success, result.Status);
        Assert.NotNull(result.Reservation);
        Assert.Equal(orderId, result.Reservation.OrderId);
        Assert.Equal(itemId, result.Reservation.StockItemId);
        Assert.Equal(20, result.Reservation.Quantity);

        var item = await context.StockItems.FindAsync(itemId);
        Assert.NotNull(item);
        Assert.Equal(30, item.QuantityReserved);

        Assert.Single(context.StockReservations);
    }

    [Fact]
    public async Task Handle_WithNonExistingSku_ReturnsStockItemNotFound()
    {
        var options = CreateInMemoryOptions(nameof(Handle_WithNonExistingSku_ReturnsStockItemNotFound));
        using var context = new InventoryDbContext(options);

        var handler = new ReserveStockCommandHandler(context);
        var result = await handler.Handle(
            new ReserveStockCommand(Guid.NewGuid(), "NO-SKU", Guid.NewGuid(), 5),
            CancellationToken.None);

        Assert.Equal(ReserveStockStatus.StockItemNotFound, result.Status);
        Assert.Null(result.Reservation);
        Assert.Empty(context.StockReservations);
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ReturnsInsufficientStock()
    {
        var options = CreateInMemoryOptions(nameof(Handle_WithInsufficientStock_ReturnsInsufficientStock));
        using var context = new InventoryDbContext(options);
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        context.StockItems.Add(new StockItem
        {
            Id = itemId,
            TenantId = tenantId,
            ProductVariantId = Guid.NewGuid(),
            Sku = "RES-002",
            QuantityAvailable = 15,
            QuantityReserved = 10
        });
        await context.SaveChangesAsync();

        var handler = new ReserveStockCommandHandler(context);
        var result = await handler.Handle(
            new ReserveStockCommand(tenantId, "RES-002", Guid.NewGuid(), 10),
            CancellationToken.None);

        Assert.Equal(ReserveStockStatus.InsufficientStock, result.Status);
        Assert.Null(result.Reservation);

        var item = await context.StockItems.FindAsync(itemId);
        Assert.NotNull(item);
        Assert.Equal(10, item.QuantityReserved);
        Assert.Empty(context.StockReservations);
    }
}
