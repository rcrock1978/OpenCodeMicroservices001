using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Tenant"/> entity and related configurations.
/// </summary>
public class TenantTests
{
    #region Tenant Creation & Properties

    [Fact]
    public void Tenant_Created_WithRequiredProperties_ShouldSucceed()
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Acme Corp",
            Subdomain = "acme"
        };

        Assert.NotEqual(Guid.Empty, tenant.Id);
        Assert.Equal("Acme Corp", tenant.Name);
        Assert.Equal("acme", tenant.Subdomain);
    }

    [Fact]
    public void Tenant_DefaultValues_ShouldBeSet()
    {
        var tenant = new Tenant
        {
            Name = "Test Tenant",
            Subdomain = "test"
        };

        Assert.True(tenant.IsActive);
        Assert.True(tenant.CreatedAt <= DateTime.UtcNow);
        Assert.Null(tenant.SubscriptionPlanId);
        Assert.NotNull(tenant.Users);
        Assert.Empty(tenant.Users);
    }

    [Fact]
    public void Tenant_SubscriptionPlanId_CanBeSet()
    {
        var tenant = new Tenant
        {
            Name = "Premium Tenant",
            Subdomain = "premium",
            SubscriptionPlanId = "plan_premium_001"
        };

        Assert.Equal("plan_premium_001", tenant.SubscriptionPlanId);
    }

    [Fact]
    public void Tenant_IsActive_CanBeSetToFalse()
    {
        var tenant = new Tenant
        {
            Name = "Inactive Tenant",
            Subdomain = "inactive",
            IsActive = false
        };

        Assert.False(tenant.IsActive);
    }

    #endregion

    #region TenantConfiguration

    [Fact]
    public void TenantConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestIdentityDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Tenant));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void TenantConfiguration_HasUniqueIndexOnSubdomain()
    {
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestIdentityDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Tenant));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var subdomainIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == "Subdomain");

        Assert.NotNull(subdomainIndex);
        Assert.True(subdomainIndex.IsUnique);
    }

    [Fact]
    public void TenantConfiguration_Name_HasMaxLength200AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestIdentityDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Tenant));

        Assert.NotNull(entityType);
        var nameProperty = entityType.FindProperty("Name");
        Assert.NotNull(nameProperty);
        Assert.Equal(200, nameProperty.GetMaxLength());
        Assert.False(nameProperty.IsNullable);
    }

    [Fact]
    public void TenantConfiguration_Subdomain_HasMaxLength100AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestIdentityDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Tenant));

        Assert.NotNull(entityType);
        var subdomainProperty = entityType.FindProperty("Subdomain");
        Assert.NotNull(subdomainProperty);
        Assert.Equal(100, subdomainProperty.GetMaxLength());
        Assert.False(subdomainProperty.IsNullable);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Tenant_WithUsers_CanBeConstructed()
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Tenant With Users",
            Subdomain = "users"
        };

        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "user1@example.com",
            PasswordHash = "hash1",
            DisplayName = "User One",
            TenantId = tenant.Id
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "user2@example.com",
            PasswordHash = "hash2",
            DisplayName = "User Two",
            TenantId = tenant.Id
        };

        tenant.Users.Add(user1);
        tenant.Users.Add(user2);

        Assert.Equal(2, tenant.Users.Count);
        Assert.All(tenant.Users, u => Assert.Equal(tenant.Id, u.TenantId));
    }

    [Fact]
    public void TenantConfiguration_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantId = Guid.NewGuid();

        using (var context = new TestIdentityDbContext(options))
        {
            context.Tenants.Add(new Tenant
            {
                Id = tenantId,
                Name = "Persisted Tenant",
                Subdomain = "persisted",
                SubscriptionPlanId = "plan_123"
            });
            context.SaveChanges();
        }

        using (var context = new TestIdentityDbContext(options))
        {
            var tenant = context.Tenants.Find(tenantId);

            Assert.NotNull(tenant);
            Assert.Equal("Persisted Tenant", tenant.Name);
            Assert.Equal("persisted", tenant.Subdomain);
            Assert.Equal("plan_123", tenant.SubscriptionPlanId);
            Assert.True(tenant.IsActive);
        }
    }

    [Fact]
    public void TenantConfiguration_SubdomainIndex_IsUnique()
    {
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestIdentityDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Tenant));

        Assert.NotNull(entityType);
        var subdomainIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties[0].Name == "Subdomain");

        Assert.NotNull(subdomainIndex);
        Assert.True(subdomainIndex.IsUnique);
    }

    #endregion
}

/// <summary>
/// In-memory test DbContext for verifying EF Core configurations.
/// </summary>
public class TestIdentityDbContext : DbContext
{
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    public TestIdentityDbContext(DbContextOptions<TestIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
