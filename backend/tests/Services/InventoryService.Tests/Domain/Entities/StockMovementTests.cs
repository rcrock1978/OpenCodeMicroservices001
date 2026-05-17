using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="StockMovement"/> entity and related configurations.
/// </summary>
public class StockMovementTests
{
    #region StockMovement Creation & Properties

    [Fact]
    public void StockMovement_Created_WithRequiredProperties_ShouldSucceed()
    {
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            StockItemId = Guid.NewGuid(),
            Type = StockMovementType.Inbound,
            Quantity = 50
        };

        Assert.NotEqual(Guid.Empty, movement.Id);
        Assert.NotEqual(Guid.Empty, movement.TenantId);
        Assert.NotEqual(Guid.Empty, movement.StockItemId);
        Assert.Equal(StockMovementType.Inbound, movement.Type);
        Assert.Equal(50, movement.Quantity);
    }

    [Fact]
    public void StockMovement_DefaultValues_ShouldBeSet()
    {
        var movement = new StockMovement
        {
            TenantId = Guid.NewGuid(),
            StockItemId = Guid.NewGuid(),
            Type = StockMovementType.Adjustment,
            Quantity = 10
        };

        Assert.True(movement.CreatedAt <= DateTime.UtcNow);
        Assert.Null(movement.Reference);
    }

    [Fact]
    public void StockMovement_Reference_CanBeSet()
    {
        var movement = new StockMovement
        {
            TenantId = Guid.NewGuid(),
            StockItemId = Guid.NewGuid(),
            Type = StockMovementType.Outbound,
            Quantity = 20,
            Reference = "Order #12345"
        };

        Assert.Equal("Order #12345", movement.Reference);
    }

    [Theory]
    [InlineData(StockMovementType.Inbound)]
    [InlineData(StockMovementType.Outbound)]
    [InlineData(StockMovementType.Adjustment)]
    [InlineData(StockMovementType.Reservation)]
    [InlineData(StockMovementType.Release)]
    public void StockMovementType_ValidValues_ShouldExist(StockMovementType type)
    {
        var movement = new StockMovement
        {
            TenantId = Guid.NewGuid(),
            StockItemId = Guid.NewGuid(),
            Type = type,
            Quantity = 1
        };

        Assert.Equal(type, movement.Type);
    }

    [Fact]
    public void StockMovementType_HasExactlyFiveValues()
    {
        var values = Enum.GetValues<StockMovementType>();

        Assert.Equal(5, values.Length);
    }

    #endregion

    #region StockMovementConfiguration

    [Fact]
    public void StockMovementConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockMovement));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void StockMovementConfiguration_HasIndexOnTenantIdAndStockItemId()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockMovement));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantItemIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "StockItemId"));

        Assert.NotNull(tenantItemIndex);
    }

    [Fact]
    public void StockMovementConfiguration_HasIndexOnCreatedAt()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockMovement));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var createdAtIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == "CreatedAt");

        Assert.NotNull(createdAtIndex);
    }

    [Fact]
    public void StockMovementConfiguration_HasForeignKeyToStockItem()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockMovement));

        Assert.NotNull(entityType);
        var foreignKey = entityType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(StockItem));

        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void StockMovement_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var movementId = Guid.NewGuid();

        using (var context = new TestInventoryDbContext(options))
        {
            context.StockMovements.Add(new StockMovement
            {
                Id = movementId,
                TenantId = Guid.NewGuid(),
                StockItemId = Guid.NewGuid(),
                Type = StockMovementType.Adjustment,
                Quantity = 25,
                Reference = "Inventory adjustment"
            });
            context.SaveChanges();
        }

        using (var context = new TestInventoryDbContext(options))
        {
            var movement = context.StockMovements.Find(movementId);

            Assert.NotNull(movement);
            Assert.Equal(StockMovementType.Adjustment, movement.Type);
            Assert.Equal(25, movement.Quantity);
            Assert.Equal("Inventory adjustment", movement.Reference);
        }
    }

    [Fact]
    public void StockMovement_MultipleMovements_CanBeAdded()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        using (var context = new TestInventoryDbContext(options))
        {
            context.StockMovements.AddRange(
                new StockMovement { Id = Guid.NewGuid(), TenantId = tenantId, StockItemId = itemId, Type = StockMovementType.Inbound, Quantity = 100 },
                new StockMovement { Id = Guid.NewGuid(), TenantId = tenantId, StockItemId = itemId, Type = StockMovementType.Reservation, Quantity = 10 },
                new StockMovement { Id = Guid.NewGuid(), TenantId = tenantId, StockItemId = itemId, Type = StockMovementType.Release, Quantity = 10 }
            );
            context.SaveChanges();
        }

        using (var context = new TestInventoryDbContext(options))
        {
            var movements = context.StockMovements.Where(m => m.StockItemId == itemId).ToList();
            Assert.Equal(3, movements.Count);
        }
    }

    #endregion
}
