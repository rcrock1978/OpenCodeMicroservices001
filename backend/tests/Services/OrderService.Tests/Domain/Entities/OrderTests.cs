using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using Xunit;

namespace OrderService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Order"/> and <see cref="OrderItem"/> entities.
/// </summary>
public class OrderTests
{
    #region Order Creation & Properties

    [Fact]
    public void Order_Created_WithRequiredProperties_ShouldSucceed()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            OrderNumber = "ORD-2024-001",
            Subtotal = 100.00m,
            ShippingCost = 10.00m,
            TaxAmount = 8.50m,
            Total = 118.50m
        };

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.NotEqual(Guid.Empty, order.TenantId);
        Assert.NotEqual(Guid.Empty, order.CustomerId);
        Assert.Equal("ORD-2024-001", order.OrderNumber);
        Assert.Equal(118.50m, order.Total);
    }

    [Fact]
    public void Order_DefaultValues_ShouldBeSet()
    {
        var order = new Order
        {
            OrderNumber = "ORD-001"
        };

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal("USD", order.Currency);
        Assert.True(order.CreatedAt <= DateTime.UtcNow);
        Assert.Null(order.ShippingAddress);
        Assert.NotNull(order.Items);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void Order_Status_CanBeChanged()
    {
        var order = new Order
        {
            OrderNumber = "ORD-002",
            Status = OrderStatus.Paid
        };

        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void Order_ShippingAddress_CanBeSet()
    {
        var order = new Order
        {
            OrderNumber = "ORD-003",
            ShippingAddress = "{ \"street\": \"123 Main St\", \"city\": \"NYC\" }"
        };

        Assert.NotNull(order.ShippingAddress);
    }

    #endregion

    #region OrderConfiguration

    [Fact]
    public void OrderConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Order));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void OrderConfiguration_HasUniqueIndexOnTenantIdAndOrderNumber()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Order));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantOrderIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "OrderNumber"));

        Assert.NotNull(tenantOrderIndex);
        Assert.True(tenantOrderIndex.IsUnique);
    }

    [Fact]
    public void OrderConfiguration_OrderNumber_HasMaxLength50AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Order));

        Assert.NotNull(entityType);
        var orderNumberProperty = entityType.FindProperty("OrderNumber");
        Assert.NotNull(orderNumberProperty);
        Assert.Equal(50, orderNumberProperty.GetMaxLength());
        Assert.False(orderNumberProperty.IsNullable);
    }

    [Fact]
    public void OrderConfiguration_Currency_HasMaxLength3AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Order));

        Assert.NotNull(entityType);
        var currencyProperty = entityType.FindProperty("Currency");
        Assert.NotNull(currencyProperty);
        Assert.Equal(3, currencyProperty.GetMaxLength());
        Assert.False(currencyProperty.IsNullable);
    }

    #endregion

    #region OrderItem Tests

    [Fact]
    public void OrderItem_Created_WithRequiredProperties_ShouldSucceed()
    {
        var item = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductVariantId = Guid.NewGuid(),
            ProductName = "Test Product",
            Sku = "PROD-001",
            UnitPrice = 49.99m,
            Quantity = 2,
            LineTotal = 99.98m
        };

        Assert.Equal("Test Product", item.ProductName);
        Assert.Equal("PROD-001", item.Sku);
        Assert.Equal(99.98m, item.LineTotal);
    }

    [Fact]
    public void OrderItemConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(OrderItem));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    [Fact]
    public void OrderItemConfiguration_HasForeignKeyToOrder()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(OrderItem));

        Assert.NotNull(entityType);
        var foreignKey = entityType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Order));

        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    #endregion

    #region OrderStatus Enum Tests

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.InventoryReserved)]
    [InlineData(OrderStatus.PaymentInitiated)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Refunded)]
    public void OrderStatus_ValidValues_ShouldExist(OrderStatus status)
    {
        var order = new Order
        {
            OrderNumber = "ORD-ENUM",
            Status = status
        };

        Assert.Equal(status, order.Status);
    }

    [Fact]
    public void OrderStatus_Pending_IsDefault()
    {
        var order = new Order
        {
            OrderNumber = "ORD-DEFAULT"
        };

        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void OrderStatus_HasExactlyEightValues()
    {
        var values = Enum.GetValues<OrderStatus>();
        Assert.Equal(8, values.Length);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Order_WithItems_CanBeConstructed()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            OrderNumber = "ORD-ITEMS-001",
            Subtotal = 100.00m,
            ShippingCost = 10.00m,
            TaxAmount = 8.00m,
            Total = 118.00m
        };

        var item1 = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductName = "Product A",
            Sku = "A-001",
            UnitPrice = 25.00m,
            Quantity = 2,
            LineTotal = 50.00m
        };

        var item2 = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductName = "Product B",
            Sku = "B-001",
            UnitPrice = 50.00m,
            Quantity = 1,
            LineTotal = 50.00m
        };

        order.Items.Add(item1);
        order.Items.Add(item2);

        Assert.Equal(2, order.Items.Count);
        Assert.Equal(100.00m, order.Items.Sum(i => i.LineTotal));
    }

    [Fact]
    public void OrderConfiguration_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var orderId = Guid.NewGuid();

        using (var context = new TestOrderDbContext(options))
        {
            context.Orders.Add(new Order
            {
                Id = orderId,
                TenantId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                OrderNumber = "PERS-001",
                Subtotal = 200.00m,
                ShippingCost = 15.00m,
                TaxAmount = 16.00m,
                Total = 231.00m,
                Status = OrderStatus.Paid
            });
            context.SaveChanges();
        }

        using (var context = new TestOrderDbContext(options))
        {
            var order = context.Orders.Find(orderId);

            Assert.NotNull(order);
            Assert.Equal("PERS-001", order.OrderNumber);
            Assert.Equal(231.00m, order.Total);
            Assert.Equal(OrderStatus.Paid, order.Status);
        }
    }

    [Fact]
    public void OrderConfiguration_TenantOrderNumberIndex_IsUnique()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Order));

        Assert.NotNull(entityType);
        var tenantOrderIndex = entityType.GetIndexes().FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "OrderNumber"));

        Assert.NotNull(tenantOrderIndex);
        Assert.True(tenantOrderIndex.IsUnique);
    }

    #endregion
}

/// <summary>
/// In-memory test DbContext for verifying EF Core configurations.
/// </summary>
public class TestOrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; } = null!;

    public TestOrderDbContext(DbContextOptions<TestOrderDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderStatusHistoryConfiguration());
    }
}
