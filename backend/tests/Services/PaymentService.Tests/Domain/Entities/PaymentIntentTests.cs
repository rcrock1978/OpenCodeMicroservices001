using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using Xunit;

namespace PaymentService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="PaymentIntent"/> and <see cref="PaymentMethod"/> entities.
/// </summary>
public class PaymentIntentTests
{
    #region PaymentIntent Creation & Properties

    [Fact]
    public void PaymentIntent_Created_WithRequiredProperties_ShouldSucceed()
    {
        var intent = new PaymentIntent
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Amount = 199.99m,
            Currency = "USD",
            IdempotencyKey = "key_123"
        };

        Assert.NotEqual(Guid.Empty, intent.Id);
        Assert.Equal(199.99m, intent.Amount);
        Assert.Equal("USD", intent.Currency);
        Assert.Equal("key_123", intent.IdempotencyKey);
    }

    [Fact]
    public void PaymentIntent_DefaultValues_ShouldBeSet()
    {
        var intent = new PaymentIntent
        {
            IdempotencyKey = "key_default"
        };

        Assert.Equal(PaymentStatus.Pending, intent.Status);
        Assert.Equal("USD", intent.Currency);
        Assert.Null(intent.PaymentMethod);
        Assert.Null(intent.FailureReason);
        Assert.Null(intent.CapturedAt);
        Assert.True(intent.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void PaymentIntent_Status_CanBeChanged()
    {
        var intent = new PaymentIntent
        {
            IdempotencyKey = "key_status",
            Status = PaymentStatus.Succeeded
        };

        Assert.Equal(PaymentStatus.Succeeded, intent.Status);
    }

    [Fact]
    public void PaymentIntent_CapturedAt_CanBeSet()
    {
        var capturedAt = DateTime.UtcNow;
        var intent = new PaymentIntent
        {
            IdempotencyKey = "key_captured",
            CapturedAt = capturedAt
        };

        Assert.Equal(capturedAt, intent.CapturedAt);
    }

    #endregion

    #region PaymentIntentConfiguration

    [Fact]
    public void PaymentIntentConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestPaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestPaymentDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(PaymentIntent));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    [Fact]
    public void PaymentIntentConfiguration_HasUniqueIndexOnTenantIdAndIdempotencyKey()
    {
        var options = new DbContextOptionsBuilder<TestPaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestPaymentDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(PaymentIntent));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantKeyIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "IdempotencyKey"));

        Assert.NotNull(tenantKeyIndex);
        Assert.True(tenantKeyIndex.IsUnique);
    }

    [Fact]
    public void PaymentIntentConfiguration_IdempotencyKey_HasMaxLength100AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestPaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestPaymentDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(PaymentIntent));

        Assert.NotNull(entityType);
        var keyProperty = entityType.FindProperty("IdempotencyKey");
        Assert.NotNull(keyProperty);
        Assert.Equal(100, keyProperty.GetMaxLength());
        Assert.False(keyProperty.IsNullable);
    }

    #endregion

    #region PaymentMethod Tests

    [Fact]
    public void PaymentMethod_Created_WithRequiredProperties_ShouldSucceed()
    {
        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Type = PaymentMethodType.Card,
            LastFour = "4242",
            Brand = "Visa",
            ExpMonth = 12,
            ExpYear = 2027
        };

        Assert.Equal("4242", method.LastFour);
        Assert.Equal("Visa", method.Brand);
        Assert.Equal(12, method.ExpMonth);
        Assert.Equal(2027, method.ExpYear);
    }

    [Fact]
    public void PaymentMethod_DefaultValues_ShouldBeSet()
    {
        var method = new PaymentMethod
        {
            TenantId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid()
        };

        Assert.Equal(PaymentMethodType.Card, method.Type);
        Assert.False(method.IsDefault);
    }

    #endregion

    #region PaymentMethodConfiguration

    [Fact]
    public void PaymentMethodConfiguration_HasIndexOnTenantIdAndCustomerId()
    {
        var options = new DbContextOptionsBuilder<TestPaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestPaymentDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(PaymentMethod));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantCustomerIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "CustomerId"));

        Assert.NotNull(tenantCustomerIndex);
    }

    #endregion

    #region PaymentStatus Enum Tests

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Processing)]
    [InlineData(PaymentStatus.Succeeded)]
    [InlineData(PaymentStatus.Failed)]
    [InlineData(PaymentStatus.Cancelled)]
    [InlineData(PaymentStatus.Refunded)]
    public void PaymentStatus_ValidValues_ShouldExist(PaymentStatus status)
    {
        var intent = new PaymentIntent
        {
            IdempotencyKey = "key_enum",
            Status = status
        };

        Assert.Equal(status, intent.Status);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void PaymentIntent_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestPaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var intentId = Guid.NewGuid();

        using (var context = new TestPaymentDbContext(options))
        {
            context.PaymentIntents.Add(new PaymentIntent
            {
                Id = intentId,
                TenantId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Amount = 500.00m,
                Currency = "EUR",
                IdempotencyKey = "unique_key_001",
                Status = PaymentStatus.Succeeded
            });
            context.SaveChanges();
        }

        using (var context = new TestPaymentDbContext(options))
        {
            var intent = context.PaymentIntents.Find(intentId);
            Assert.NotNull(intent);
            Assert.Equal(500.00m, intent.Amount);
            Assert.Equal("EUR", intent.Currency);
            Assert.Equal(PaymentStatus.Succeeded, intent.Status);
        }
    }

    [Fact]
    public void PaymentIntentConfiguration_TenantIdempotencyKeyIndex_IsUnique()
    {
        var options = new DbContextOptionsBuilder<TestPaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestPaymentDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(PaymentIntent));

        Assert.NotNull(entityType);
        var tenantKeyIndex = entityType.GetIndexes().FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "IdempotencyKey"));

        Assert.NotNull(tenantKeyIndex);
        Assert.True(tenantKeyIndex.IsUnique);
    }

    #endregion
}

/// <summary>
/// In-memory test DbContext for verifying EF Core configurations.
/// </summary>
public class TestPaymentDbContext : DbContext
{
    public DbSet<PaymentIntent> PaymentIntents { get; set; } = null!;
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;

    public TestPaymentDbContext(DbContextOptions<TestPaymentDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PaymentIntentConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentTransactionConfiguration());
    }
}
