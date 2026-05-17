using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Customer"/> entity and related configurations.
/// </summary>
public class CustomerTests
{
    #region Customer Creation & Properties

    [Fact]
    public void Customer_Created_WithRequiredProperties_ShouldSucceed()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.NotEqual(Guid.Empty, customer.TenantId);
        Assert.Equal("john.doe@example.com", customer.Email);
        Assert.Equal("John", customer.FirstName);
        Assert.Equal("Doe", customer.LastName);
    }

    [Fact]
    public void Customer_DefaultValues_ShouldBeSet()
    {
        var customer = new Customer
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        Assert.True(customer.CreatedAt <= DateTime.UtcNow);
        Assert.Null(customer.UserId);
        Assert.Null(customer.PhoneNumber);
        Assert.NotNull(customer.Addresses);
        Assert.Empty(customer.Addresses);
    }

    [Fact]
    public void Customer_UserId_CanBeSet()
    {
        var userId = Guid.NewGuid();
        var customer = new Customer
        {
            Email = "linked@example.com",
            FirstName = "Linked",
            LastName = "User",
            UserId = userId
        };

        Assert.Equal(userId, customer.UserId);
    }

    [Fact]
    public void Customer_PhoneNumber_CanBeNull()
    {
        var customer = new Customer
        {
            Email = "nophone@example.com",
            FirstName = "No",
            LastName = "Phone"
        };

        Assert.Null(customer.PhoneNumber);
    }

    [Fact]
    public void Customer_PhoneNumber_CanBeSet()
    {
        var customer = new Customer
        {
            Email = "withphone@example.com",
            FirstName = "With",
            LastName = "Phone",
            PhoneNumber = "+1-555-123-4567"
        };

        Assert.Equal("+1-555-123-4567", customer.PhoneNumber);
    }

    #endregion

    #region CustomerConfiguration

    [Fact]
    public void CustomerConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Customer));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void CustomerConfiguration_HasUniqueIndexOnTenantIdAndEmail()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Customer));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantEmailIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "Email"));

        Assert.NotNull(tenantEmailIndex);
        Assert.True(tenantEmailIndex.IsUnique);
    }

    [Fact]
    public void CustomerConfiguration_Email_HasMaxLength256AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Customer));

        Assert.NotNull(entityType);
        var emailProperty = entityType.FindProperty("Email");
        Assert.NotNull(emailProperty);
        Assert.Equal(256, emailProperty.GetMaxLength());
        Assert.False(emailProperty.IsNullable);
    }

    [Fact]
    public void CustomerConfiguration_FirstName_HasMaxLength100AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Customer));

        Assert.NotNull(entityType);
        var firstNameProperty = entityType.FindProperty("FirstName");
        Assert.NotNull(firstNameProperty);
        Assert.Equal(100, firstNameProperty.GetMaxLength());
        Assert.False(firstNameProperty.IsNullable);
    }

    [Fact]
    public void CustomerConfiguration_LastName_HasMaxLength100AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Customer));

        Assert.NotNull(entityType);
        var lastNameProperty = entityType.FindProperty("LastName");
        Assert.NotNull(lastNameProperty);
        Assert.Equal(100, lastNameProperty.GetMaxLength());
        Assert.False(lastNameProperty.IsNullable);
    }

    #endregion

    #region Address Tests

    [Fact]
    public void Address_Created_WithRequiredProperties_ShouldSucceed()
    {
        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Street = "123 Main St",
            City = "New York",
            PostalCode = "10001",
            Country = "US"
        };

        Assert.NotEqual(Guid.Empty, address.Id);
        Assert.NotEqual(Guid.Empty, address.CustomerId);
        Assert.Equal("123 Main St", address.Street);
        Assert.Equal("New York", address.City);
        Assert.Equal("10001", address.PostalCode);
        Assert.Equal("US", address.Country);
    }

    [Fact]
    public void Address_DefaultValues_ShouldBeSet()
    {
        var address = new Address
        {
            Street = "456 Oak Ave",
            City = "Los Angeles",
            PostalCode = "90001",
            Country = "US"
        };

        Assert.Equal(AddressType.Shipping, address.Type);
        Assert.False(address.IsDefault);
        Assert.Null(address.State);
    }

    [Fact]
    public void Address_Type_CanBeSetToBilling()
    {
        var address = new Address
        {
            Street = "789 Pine Rd",
            City = "Chicago",
            PostalCode = "60601",
            Country = "US",
            Type = AddressType.Billing
        };

        Assert.Equal(AddressType.Billing, address.Type);
    }

    [Fact]
    public void Address_State_CanBeSet()
    {
        var address = new Address
        {
            Street = "321 Elm St",
            City = "Austin",
            State = "TX",
            PostalCode = "78701",
            Country = "US"
        };

        Assert.Equal("TX", address.State);
    }

    [Fact]
    public void Address_IsDefault_CanBeSet()
    {
        var address = new Address
        {
            Street = "555 Default Ln",
            City = "Seattle",
            PostalCode = "98101",
            Country = "US",
            IsDefault = true
        };

        Assert.True(address.IsDefault);
    }

    #endregion

    #region AddressConfiguration

    [Fact]
    public void AddressConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Address));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void AddressConfiguration_HasForeignKeyToCustomer()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Address));

        Assert.NotNull(entityType);
        var foreignKey = entityType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void AddressConfiguration_Street_HasMaxLength300AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Address));

        Assert.NotNull(entityType);
        var streetProperty = entityType.FindProperty("Street");
        Assert.NotNull(streetProperty);
        Assert.Equal(300, streetProperty.GetMaxLength());
        Assert.False(streetProperty.IsNullable);
    }

    [Fact]
    public void AddressConfiguration_City_HasMaxLength100AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Address));

        Assert.NotNull(entityType);
        var cityProperty = entityType.FindProperty("City");
        Assert.NotNull(cityProperty);
        Assert.Equal(100, cityProperty.GetMaxLength());
        Assert.False(cityProperty.IsNullable);
    }

    [Fact]
    public void AddressConfiguration_PostalCode_HasMaxLength20AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Address));

        Assert.NotNull(entityType);
        var postalCodeProperty = entityType.FindProperty("PostalCode");
        Assert.NotNull(postalCodeProperty);
        Assert.Equal(20, postalCodeProperty.GetMaxLength());
        Assert.False(postalCodeProperty.IsNullable);
    }

    [Fact]
    public void AddressConfiguration_Country_HasMaxLength2AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Address));

        Assert.NotNull(entityType);
        var countryProperty = entityType.FindProperty("Country");
        Assert.NotNull(countryProperty);
        Assert.Equal(2, countryProperty.GetMaxLength());
        Assert.False(countryProperty.IsNullable);
    }

    #endregion

    #region AddressType Enum Tests

    [Theory]
    [InlineData(AddressType.Shipping)]
    [InlineData(AddressType.Billing)]
    public void AddressType_ValidValues_ShouldExist(AddressType type)
    {
        var address = new Address
        {
            Street = "123 Test St",
            City = "Test City",
            PostalCode = "12345",
            Country = "US",
            Type = type
        };

        Assert.Equal(type, address.Type);
    }

    [Fact]
    public void AddressType_Shipping_IsDefault()
    {
        var address = new Address
        {
            Street = "123 Test St",
            City = "Test City",
            PostalCode = "12345",
            Country = "US"
        };

        Assert.Equal(AddressType.Shipping, address.Type);
    }

    [Fact]
    public void AddressType_HasExactlyTwoValues()
    {
        var values = Enum.GetValues<AddressType>();

        Assert.Equal(2, values.Length);
        Assert.Contains(AddressType.Shipping, values);
        Assert.Contains(AddressType.Billing, values);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Customer_WithAddresses_FullGraph_CanBeConstructed()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "richard@example.com",
            FirstName = "Richard",
            LastName = "Roe",
            PhoneNumber = "+1-555-999-8888"
        };

        var shippingAddress = new Address
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Type = AddressType.Shipping,
            Street = "100 Shipping Lane",
            City = "Boston",
            State = "MA",
            PostalCode = "02101",
            Country = "US",
            IsDefault = true
        };

        var billingAddress = new Address
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Type = AddressType.Billing,
            Street = "200 Billing Blvd",
            City = "Boston",
            State = "MA",
            PostalCode = "02101",
            Country = "US",
            IsDefault = false
        };

        customer.Addresses.Add(shippingAddress);
        customer.Addresses.Add(billingAddress);

        Assert.Equal(2, customer.Addresses.Count);
        Assert.Contains(shippingAddress, customer.Addresses);
        Assert.Contains(billingAddress, customer.Addresses);
        Assert.All(customer.Addresses, a => Assert.Equal(customer.Id, a.CustomerId));
    }

    [Fact]
    public void CustomerConfiguration_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var customerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        using (var context = new TestCustomerDbContext(options))
        {
            context.Customers.Add(new Customer
            {
                Id = customerId,
                TenantId = tenantId,
                Email = "persisted@example.com",
                FirstName = "Persisted",
                LastName = "Customer",
                PhoneNumber = "+1-555-000-1111"
            });
            context.SaveChanges();
        }

        using (var context = new TestCustomerDbContext(options))
        {
            var customer = context.Customers.Find(customerId);

            Assert.NotNull(customer);
            Assert.Equal("persisted@example.com", customer.Email);
            Assert.Equal("Persisted", customer.FirstName);
            Assert.Equal("Customer", customer.LastName);
            Assert.Equal("+1-555-000-1111", customer.PhoneNumber);
            Assert.Equal(tenantId, customer.TenantId);
        }
    }

    [Fact]
    public void CustomerConfiguration_TenantEmailIndex_IsUnique()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Customer));

        Assert.NotNull(entityType);
        var tenantEmailIndex = entityType.GetIndexes().FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "Email"));

        Assert.NotNull(tenantEmailIndex);
        Assert.True(tenantEmailIndex.IsUnique);
    }

    [Fact]
    public void Address_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var addressId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        using (var context = new TestCustomerDbContext(options))
        {
            context.Addresses.Add(new Address
            {
                Id = addressId,
                CustomerId = customerId,
                Type = AddressType.Billing,
                Street = "456 Saved Ave",
                City = "Saved City",
                State = "CA",
                PostalCode = "90210",
                Country = "US",
                IsDefault = true
            });
            context.SaveChanges();
        }

        using (var context = new TestCustomerDbContext(options))
        {
            var address = context.Addresses.Find(addressId);

            Assert.NotNull(address);
            Assert.Equal("456 Saved Ave", address.Street);
            Assert.Equal("Saved City", address.City);
            Assert.Equal("CA", address.State);
            Assert.Equal("90210", address.PostalCode);
            Assert.Equal("US", address.Country);
            Assert.Equal(AddressType.Billing, address.Type);
            Assert.True(address.IsDefault);
        }
    }

    [Fact]
    public void CustomerAndAddress_Relationship_PersistedTogether()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var customerId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        using (var context = new TestCustomerDbContext(options))
        {
            var customer = new Customer
            {
                Id = customerId,
                TenantId = tenantId,
                Email = "withaddress@example.com",
                FirstName = "With",
                LastName = "Address"
            };

            var address = new Address
            {
                Id = addressId,
                CustomerId = customerId,
                Street = "123 Related St",
                City = "Related City",
                PostalCode = "12345",
                Country = "US"
            };

            context.Customers.Add(customer);
            context.Addresses.Add(address);
            context.SaveChanges();
        }

        using (var context = new TestCustomerDbContext(options))
        {
            var customer = context.Customers.Find(customerId);
            var address = context.Addresses.Find(addressId);

            Assert.NotNull(customer);
            Assert.NotNull(address);
            Assert.Equal(customerId, address.CustomerId);
        }
    }

    #endregion
}

/// <summary>
/// In-memory test DbContext for verifying EF Core configurations.
/// </summary>
public class TestCustomerDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;

    public TestCustomerDbContext(DbContextOptions<TestCustomerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
    }
}
