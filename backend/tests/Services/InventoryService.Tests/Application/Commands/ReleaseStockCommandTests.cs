using InventoryService.Application.Commands;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryService.Tests.Application.Commands;

/// <summary>
/// Unit tests for the <see cref="ReleaseStockCommandHandler"/>.
/// </summary>
public class ReleaseStockCommandTests
{
    private static DbContextOptions<InventoryDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task Handle_WithExistingReservation_ReleasesStockAndReturnsReleased()
    {
        var options = CreateInMemoryOptions(nameof(Handle_WithExistingReservation_ReleasesStockAndReturnsReleased));
        using var context = new InventoryDbContext(options);
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        context.StockItems.Add(new StockItem
        {
            Id = itemId,
            TenantId = tenantId,
            ProductVariantId = Guid.NewGuid(),
            Sku = "REL-001",
            QuantityAvailable = 50,
            QuantityReserved = 15
        });

        context.StockReservations.Add(new StockReservation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderId = orderId,
            StockItemId = itemId,
            Quantity = 10,
            Status = ReservationStatus.Reserved,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });
        await context.SaveChangesAsync();

        var handler = new ReleaseStockCommandHandler(context);
        var result = await handler.Handle(new ReleaseStockCommand(tenantId, orderId), CancellationToken.None);

        Assert.True(result.Released);

        var item = await context.StockItems.FindAsync(itemId);
        Assert.NotNull(item);
        Assert.Equal(5, item.QuantityReserved);

        var reservation = await context.StockReservations.FirstAsync(r => r.OrderId == orderId);
        Assert.Equal(ReservationStatus.Released, reservation.Status);
    }

    [Fact]
    public async Task Handle_WithNoReservation_ReturnsNotReleased()
    {
        var options = CreateInMemoryOptions(nameof(Handle_WithNoReservation_ReturnsNotReleased));
        using var context = new InventoryDbContext(options);
        var tenantId = Guid.NewGuid();

        context.StockItems.Add(new StockItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductVariantId = Guid.NewGuid(),
            Sku = "REL-002",
            QuantityAvailable = 20,
            QuantityReserved = 0
        });
        await context.SaveChangesAsync();

        var handler = new ReleaseStockCommandHandler(context);
        var result = await handler.Handle(new ReleaseStockCommand(tenantId, Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.Released);
    }
}
