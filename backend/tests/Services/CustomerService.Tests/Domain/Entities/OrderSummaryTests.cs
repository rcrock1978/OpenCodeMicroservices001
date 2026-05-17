using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="OrderSummary"/> entity and related configurations.
/// </summary>
public class OrderSummaryTests
{
    #region OrderSummary Creation & Properties

    [Fact]
    public void OrderSummary_Created_WithRequiredProperties_ShouldSucceed()
    {
        var orderSummary = new OrderSummary
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 199.99m,
            Status = "Completed"
        };

        Assert.NotEqual(Guid.Empty, orderSummary.Id);
        Assert.NotEqual(Guid.Empty, orderSummary.TenantId);
        Assert.NotEqual(Guid.Empty, orderSummary.OrderId);
        Assert.NotEqual(Guid.Empty, orderSummary.CustomerId);
        Assert.Equal(199.99m, orderSummary.TotalAmount);
        Assert.Equal("Completed", orderSummary.Status);
    }

    [Fact]
    public void OrderSummary_DefaultValues_ShouldBeSet()
    {
        var orderSummary = new OrderSummary
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            Status = "Pending"
        };

        Assert.True(orderSummary.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void OrderSummary_TotalAmount_CanBeZero()
    {
        var orderSummary = new OrderSummary
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 0.00m,
            Status = "Cancelled"
        };

        Assert.Equal(0.00m, orderSummary.TotalAmount);
    }

    [Fact]
    public void OrderSummary_Status_CanBeVariousValues()
    {
        var statuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled", "Refunded" };

        foreach (var status in statuses)
        {
            var orderSummary = new OrderSummary
            {
                TenantId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                TotalAmount = 100.00m,
                Status = status
            };

            Assert.Equal(status, orderSummary.Status);
        }
    }

    #endregion

    #region OrderSummary Relationships

    [Fact]
    public void OrderSummary_Customer_CanBeAssigned()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var orderSummary = new OrderSummary
        {
            TenantId = customer.TenantId,
            OrderId = Guid.NewGuid(),
            CustomerId = customer.Id,
            Customer = customer,
            TotalAmount = 150.00m,
            Status = "Completed"
        };

        Assert.NotNull(orderSummary.Customer);
        Assert.Equal(customer.Id, orderSummary.CustomerId);
        Assert.Equal("test@example.com", orderSummary.Customer.Email);
    }

    #endregion

    #region OrderSummaryConfiguration

    [Fact]
    public void OrderSummaryConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestOrderSummaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderSummaryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(OrderSummary));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void OrderSummaryConfiguration_HasUniqueIndexOnTenantIdAndOrderId()
    {
        var options = new DbContextOptionsBuilder<TestOrderSummaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderSummaryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(OrderSummary));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantOrderIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "OrderId"));

        Assert.NotNull(tenantOrderIndex);
        Assert.True(tenantOrderIndex.IsUnique);
    }

    [Fact]
    public void OrderSummaryConfiguration_HasIndexOnTenantIdAndCustomerId()
    {
        var options = new DbContextOptionsBuilder<TestOrderSummaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderSummaryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(OrderSummary));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantCustomerIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "CustomerId"));

        Assert.NotNull(tenantCustomerIndex);
    }

    [Fact]
    public void OrderSummaryConfiguration_HasForeignKeyToCustomer()
    {
        var options = new DbContextOptionsBuilder<TestOrderSummaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderSummaryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(OrderSummary));

        Assert.NotNull(entityType);
        var foreignKey = entityType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void OrderSummary_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestOrderSummaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var orderSummaryId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        using (var context = new TestOrderSummaryDbContext(options))
        {
            context.OrderSummaries.Add(new OrderSummary
            {
                Id = orderSummaryId,
                TenantId = tenantId,
                OrderId = orderId,
                CustomerId = customerId,
                TotalAmount = 299.99m,
                Status = "Shipped"
            });
            context.SaveChanges();
        }

        using (var context = new TestOrderSummaryDbContext(options))
        {
            var orderSummary = context.OrderSummaries.Find(orderSummaryId);

            Assert.NotNull(orderSummary);
            Assert.Equal(tenantId, orderSummary.TenantId);
            Assert.Equal(orderId, orderSummary.OrderId);
            Assert.Equal(customerId, orderSummary.CustomerId);
            Assert.Equal(299.99m, orderSummary.TotalAmount);
            Assert.Equal("Shipped", orderSummary.Status);
        }
    }

    [Fact]
    public void OrderSummaryConfiguration_TenantOrderIdIndex_IsUnique()
    {
        var options = new DbContextOptionsBuilder<TestOrderSummaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestOrderSummaryDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(OrderSummary));

        Assert.NotNull(entityType);
        var tenantOrderIndex = entityType.GetIndexes().FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "OrderId"));

        Assert.NotNull(tenantOrderIndex);
        Assert.True(tenantOrderIndex.IsUnique);
    }

    [Fact]
    public void OrderSummary_MultipleOrdersPerCustomer_CanBeAdded()
    {
        var options = new DbContextOptionsBuilder<TestOrderSummaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        using (var context = new TestOrderSummaryDbContext(options))
        {
            context.OrderSummaries.AddRange(
                new OrderSummary
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OrderId = Guid.NewGuid(),
                    CustomerId = customerId,
                    TotalAmount = 50.00m,
                    Status = "Completed"
                },
                new OrderSummary
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OrderId = Guid.NewGuid(),
                    CustomerId = customerId,
                    TotalAmount = 75.00m,
                    Status = "Completed"
                },
                new OrderSummary
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OrderId = Guid.NewGuid(),
                    CustomerId = customerId,
                    TotalAmount = 120.00m,
                    Status = "Pending"
                });
            context.SaveChanges();
        }

        using (var context = new TestOrderSummaryDbContext(options))
        {
            var customerOrders = context.OrderSummaries
                .Where(o => o.CustomerId == customerId)
                .ToList();

            Assert.Equal(3, customerOrders.Count);
            Assert.Equal(245.00m, customerOrders.Sum(o => o.TotalAmount));
        }
    }

    [Fact]
    public void OrderSummary_QueryByStatus_ShouldFilter()
    {
        var options = new DbContextOptionsBuilder<TestOrderSummaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantId = Guid.NewGuid();

        using (var context = new TestOrderSummaryDbContext(options))
        {
            context.OrderSummaries.AddRange(
                new OrderSummary
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OrderId = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    TotalAmount = 50.00m,
                    Status = "Completed"
                },
                new OrderSummary
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OrderId = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    TotalAmount = 75.00m,
                    Status = "Pending"
                },
                new OrderSummary
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OrderId = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    TotalAmount = 100.00m,
                    Status = "Completed"
                });
            context.SaveChanges();
        }

        using (var context = new TestOrderSummaryDbContext(options))
        {
            var completedOrders = context.OrderSummaries
                .Where(o => o.Status == "Completed")
                .ToList();

            var pendingOrders = context.OrderSummaries
                .Where(o => o.Status == "Pending")
                .ToList();

            Assert.Equal(2, completedOrders.Count);
            Assert.Single(pendingOrders);
        }
    }

    [Fact]
    public void OrderSummary_WithCustomer_FullGraph_CanBeConstructed()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "order.customer@example.com",
            FirstName = "Order",
            LastName = "Customer"
        };

        var orderSummary = new OrderSummary
        {
            Id = Guid.NewGuid(),
            TenantId = customer.TenantId,
            OrderId = Guid.NewGuid(),
            CustomerId = customer.Id,
            Customer = customer,
            TotalAmount = 500.00m,
            Status = "Delivered"
        };

        Assert.NotNull(orderSummary.Customer);
        Assert.Equal(customer.Id, orderSummary.CustomerId);
        Assert.Equal("order.customer@example.com", orderSummary.Customer.Email);
        Assert.Equal(500.00m, orderSummary.TotalAmount);
    }

    #endregion
}

/// <summary>
/// In-memory test DbContext for verifying EF Core configurations.
/// </summary>
public class TestOrderSummaryDbContext : DbContext
{
    public DbSet<OrderSummary> OrderSummaries { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;

    public TestOrderSummaryDbContext(DbContextOptions<TestOrderSummaryDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderSummaryConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}
