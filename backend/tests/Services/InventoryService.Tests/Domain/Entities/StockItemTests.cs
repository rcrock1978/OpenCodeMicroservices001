using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="StockItem"/> and <see cref="StockReservation"/> entities.
/// </summary>
public class StockItemTests
{
    #region StockItem Creation & Properties

    [Fact]
    public void StockItem_Created_WithRequiredProperties_ShouldSucceed()
    {
        var item = new StockItem
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            ProductVariantId = Guid.NewGuid(),
            Sku = "INV-001",
            QuantityAvailable = 100,
            QuantityReserved = 10
        };

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.NotEqual(Guid.Empty, item.TenantId);
        Assert.NotEqual(Guid.Empty, item.ProductVariantId);
        Assert.Equal("INV-001", item.Sku);
        Assert.Equal(100, item.QuantityAvailable);
        Assert.Equal(10, item.QuantityReserved);
    }

    [Fact]
    public void StockItem_DefaultValues_ShouldBeSet()
    {
        var item = new StockItem
        {
            Sku = "DEFAULT-001"
        };

        Assert.Equal(10, item.LowStockThreshold);
        Assert.Equal(0, item.QuantityAvailable);
        Assert.Equal(0, item.QuantityReserved);
        Assert.True(item.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void StockItem_LowStockThreshold_CanBeCustomized()
    {
        var item = new StockItem
        {
            Sku = "LOW-001",
            LowStockThreshold = 5
        };

        Assert.Equal(5, item.LowStockThreshold);
    }

    [Fact]
    public void StockItem_IsLowStock_WhenBelowThreshold()
    {
        var item = new StockItem
        {
            Sku = "LOW-STOCK-001",
            QuantityAvailable = 8,
            LowStockThreshold = 10
        };

        Assert.True(item.QuantityAvailable < item.LowStockThreshold);
    }

    #endregion

    #region StockItemConfiguration

    [Fact]
    public void StockItemConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockItem));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void StockItemConfiguration_HasUniqueIndexOnTenantIdAndProductVariantId()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockItem));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantVariantIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "ProductVariantId"));

        Assert.NotNull(tenantVariantIndex);
        Assert.True(tenantVariantIndex.IsUnique);
    }

    [Fact]
    public void StockItemConfiguration_HasUniqueIndexOnSku()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockItem));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var skuIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == "Sku");

        Assert.NotNull(skuIndex);
        Assert.True(skuIndex.IsUnique);
    }

    [Fact]
    public void StockItemConfiguration_Sku_HasMaxLength100AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockItem));

        Assert.NotNull(entityType);
        var skuProperty = entityType.FindProperty("Sku");
        Assert.NotNull(skuProperty);
        Assert.Equal(100, skuProperty.GetMaxLength());
        Assert.False(skuProperty.IsNullable);
    }

    #endregion

    #region StockReservation Tests

    [Fact]
    public void StockReservation_Created_WithRequiredProperties_ShouldSucceed()
    {
        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            StockItemId = Guid.NewGuid(),
            Quantity = 5,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        Assert.NotEqual(Guid.Empty, reservation.Id);
        Assert.Equal(5, reservation.Quantity);
        Assert.Equal(ReservationStatus.Reserved, reservation.Status);
    }

    [Fact]
    public void StockReservation_DefaultValues_ShouldBeSet()
    {
        var reservation = new StockReservation
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            StockItemId = Guid.NewGuid(),
            Quantity = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        Assert.Equal(ReservationStatus.Reserved, reservation.Status);
        Assert.True(reservation.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void StockReservation_Status_CanBeChanged()
    {
        var reservation = new StockReservation
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            StockItemId = Guid.NewGuid(),
            Quantity = 2,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Status = ReservationStatus.Committed
        };

        Assert.Equal(ReservationStatus.Committed, reservation.Status);
    }

    #endregion

    #region StockReservationConfiguration

    [Fact]
    public void StockReservationConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockReservation));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void StockReservationConfiguration_HasIndexOnTenantIdAndOrderId()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockReservation));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantOrderIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "OrderId"));

        Assert.NotNull(tenantOrderIndex);
    }

    [Fact]
    public void StockReservationConfiguration_HasIndexOnExpiresAt()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockReservation));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var expiresAtIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == "ExpiresAt");

        Assert.NotNull(expiresAtIndex);
    }

    #endregion

    #region ReservationStatus Enum Tests

    [Theory]
    [InlineData(ReservationStatus.Reserved)]
    [InlineData(ReservationStatus.Committed)]
    [InlineData(ReservationStatus.Released)]
    [InlineData(ReservationStatus.Expired)]
    public void ReservationStatus_ValidValues_ShouldExist(ReservationStatus status)
    {
        var reservation = new StockReservation
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            StockItemId = Guid.NewGuid(),
            Quantity = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Status = status
        };

        Assert.Equal(status, reservation.Status);
    }

    [Fact]
    public void ReservationStatus_Reserved_IsDefault()
    {
        var reservation = new StockReservation
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            StockItemId = Guid.NewGuid(),
            Quantity = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        Assert.Equal(ReservationStatus.Reserved, reservation.Status);
    }

    [Fact]
    public void ReservationStatus_HasExactlyFourValues()
    {
        var values = Enum.GetValues<ReservationStatus>();

        Assert.Equal(4, values.Length);
        Assert.Contains(ReservationStatus.Reserved, values);
        Assert.Contains(ReservationStatus.Committed, values);
        Assert.Contains(ReservationStatus.Released, values);
        Assert.Contains(ReservationStatus.Expired, values);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void StockItem_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var itemId = Guid.NewGuid();

        using (var context = new TestInventoryDbContext(options))
        {
            context.StockItems.Add(new StockItem
            {
                Id = itemId,
                TenantId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                Sku = "PERSIST-001",
                QuantityAvailable = 50,
                QuantityReserved = 5,
                LowStockThreshold = 10
            });
            context.SaveChanges();
        }

        using (var context = new TestInventoryDbContext(options))
        {
            var item = context.StockItems.Find(itemId);

            Assert.NotNull(item);
            Assert.Equal("PERSIST-001", item.Sku);
            Assert.Equal(50, item.QuantityAvailable);
            Assert.Equal(5, item.QuantityReserved);
        }
    }

    [Fact]
    public void StockItemConfiguration_SkuIndex_IsUnique()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInventoryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(StockItem));

        Assert.NotNull(entityType);
        var skuIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties[0].Name == "Sku");

        Assert.NotNull(skuIndex);
        Assert.True(skuIndex.IsUnique);
    }

    [Fact]
    public void StockReservation_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var reservationId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(30);

        using (var context = new TestInventoryDbContext(options))
        {
            context.StockReservations.Add(new StockReservation
            {
                Id = reservationId,
                TenantId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                StockItemId = Guid.NewGuid(),
                Quantity = 10,
                Status = ReservationStatus.Reserved,
                ExpiresAt = expiresAt
            });
            context.SaveChanges();
        }

        using (var context = new TestInventoryDbContext(options))
        {
            var reservation = context.StockReservations.Find(reservationId);

            Assert.NotNull(reservation);
            Assert.Equal(10, reservation.Quantity);
            Assert.Equal(ReservationStatus.Reserved, reservation.Status);
        }
    }

    #endregion
}

/// <summary>
/// In-memory test DbContext for verifying EF Core configurations.
/// </summary>
public class TestInventoryDbContext : DbContext
{
    public DbSet<StockItem> StockItems { get; set; } = null!;
    public DbSet<StockReservation> StockReservations { get; set; } = null!;
    public DbSet<StockMovement> StockMovements { get; set; } = null!;

    public TestInventoryDbContext(DbContextOptions<TestInventoryDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StockItemConfiguration());
        modelBuilder.ApplyConfiguration(new StockReservationConfiguration());
        modelBuilder.ApplyConfiguration(new StockMovementConfiguration());
    }
}
