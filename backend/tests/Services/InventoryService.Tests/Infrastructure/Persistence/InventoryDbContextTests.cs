using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryService.Tests.Infrastructure.Persistence;

/// <summary>
/// Unit tests for the <see cref="InventoryDbContext"/> database context.
/// </summary>
public class InventoryDbContextTests
{
    private static DbContextOptions<InventoryDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    #region Constructor & DbSets

    [Fact]
    public void InventoryDbContext_Constructor_WithValidOptions_ShouldCreateInstance()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new InventoryDbContext(options);

        Assert.NotNull(context);
    }

    [Fact]
    public void InventoryDbContext_StockItemsDbSet_ShouldNotBeNull()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new InventoryDbContext(options);

        Assert.NotNull(context.StockItems);
    }

    [Fact]
    public void InventoryDbContext_StockReservationsDbSet_ShouldNotBeNull()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new InventoryDbContext(options);

        Assert.NotNull(context.StockReservations);
    }

    [Fact]
    public void InventoryDbContext_StockMovementsDbSet_ShouldNotBeNull()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new InventoryDbContext(options);

        Assert.NotNull(context.StockMovements);
    }

    #endregion

    #region Model Configuration

    [Fact]
    public void InventoryDbContext_ModelCreating_AppliesStockItemConfiguration()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new InventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockItem));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    [Fact]
    public void InventoryDbContext_ModelCreating_AppliesStockReservationConfiguration()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new InventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockReservation));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    [Fact]
    public void InventoryDbContext_ModelCreating_AppliesStockMovementConfiguration()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new InventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockMovement));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    #endregion

    #region StockItem CRUD

    [Fact]
    public void InventoryDbContext_StockItem_Add_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var itemId = Guid.NewGuid();

        using (var context = new InventoryDbContext(options))
        {
            context.StockItems.Add(new StockItem
            {
                Id = itemId,
                TenantId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                Sku = "TEST-001",
                QuantityAvailable = 100,
                QuantityReserved = 0
            });
            context.SaveChanges();
        }

        using (var context = new InventoryDbContext(options))
        {
            var item = context.StockItems.Find(itemId);
            Assert.NotNull(item);
            Assert.Equal("TEST-001", item.Sku);
        }
    }

    #endregion

    #region StockReservation CRUD

    [Fact]
    public void InventoryDbContext_StockReservation_Add_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var reservationId = Guid.NewGuid();

        using (var context = new InventoryDbContext(options))
        {
            context.StockReservations.Add(new StockReservation
            {
                Id = reservationId,
                TenantId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                StockItemId = Guid.NewGuid(),
                Quantity = 5,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            });
            context.SaveChanges();
        }

        using (var context = new InventoryDbContext(options))
        {
            var reservation = context.StockReservations.Find(reservationId);
            Assert.NotNull(reservation);
            Assert.Equal(5, reservation.Quantity);
        }
    }

    #endregion

    #region StockMovement CRUD

    [Fact]
    public void InventoryDbContext_StockMovement_Add_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var movementId = Guid.NewGuid();

        using (var context = new InventoryDbContext(options))
        {
            context.StockMovements.Add(new StockMovement
            {
                Id = movementId,
                TenantId = Guid.NewGuid(),
                StockItemId = Guid.NewGuid(),
                Type = StockMovementType.Inbound,
                Quantity = 50,
                Reference = "Initial stock"
            });
            context.SaveChanges();
        }

        using (var context = new InventoryDbContext(options))
        {
            var movement = context.StockMovements.Find(movementId);
            Assert.NotNull(movement);
            Assert.Equal(StockMovementType.Inbound, movement.Type);
            Assert.Equal(50, movement.Quantity);
        }
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void InventoryDbContext_FullGraph_CanBePersisted()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var movementId = Guid.NewGuid();

        using (var context = new InventoryDbContext(options))
        {
            var item = new StockItem
            {
                Id = itemId,
                TenantId = tenantId,
                ProductVariantId = Guid.NewGuid(),
                Sku = "FULL-001",
                QuantityAvailable = 100,
                QuantityReserved = 10
            };

            var reservation = new StockReservation
            {
                Id = reservationId,
                TenantId = tenantId,
                OrderId = Guid.NewGuid(),
                StockItemId = itemId,
                Quantity = 10,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            var movement = new StockMovement
            {
                Id = movementId,
                TenantId = tenantId,
                StockItemId = itemId,
                Type = StockMovementType.Reservation,
                Quantity = 10,
                Reference = "Order 123"
            };

            context.StockItems.Add(item);
            context.StockReservations.Add(reservation);
            context.StockMovements.Add(movement);
            context.SaveChanges();
        }

        using (var context = new InventoryDbContext(options))
        {
            Assert.NotNull(context.StockItems.Find(itemId));
            Assert.NotNull(context.StockReservations.Find(reservationId));
            Assert.NotNull(context.StockMovements.Find(movementId));
        }
    }

    #endregion
}
