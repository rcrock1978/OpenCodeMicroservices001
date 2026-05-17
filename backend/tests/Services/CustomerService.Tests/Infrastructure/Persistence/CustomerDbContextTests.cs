using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerService.Tests.Infrastructure.Persistence;

/// <summary>
/// Unit tests for the <see cref="CustomerDbContext"/> database context.
/// </summary>
public class CustomerDbContextTests
{
    private static DbContextOptions<CustomerDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    #region Constructor & DbSets

    [Fact]
    public void CustomerDbContext_Constructor_WithValidOptions_ShouldCreateInstance()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CustomerDbContext(options);

        Assert.NotNull(context);
    }

    [Fact]
    public void CustomerDbContext_CustomersDbSet_ShouldNotBeNull()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CustomerDbContext(options);

        Assert.NotNull(context.Customers);
    }

    [Fact]
    public void CustomerDbContext_AddressesDbSet_ShouldNotBeNull()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CustomerDbContext(options);

        Assert.NotNull(context.Addresses);
    }

    [Fact]
    public void CustomerDbContext_OrderSummariesDbSet_ShouldNotBeNull()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CustomerDbContext(options);

        Assert.NotNull(context.OrderSummaries);
    }

    #endregion

    #region Model Configuration

    [Fact]
    public void CustomerDbContext_ModelCreating_AppliesCustomerConfiguration()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Customer));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    [Fact]
    public void CustomerDbContext_ModelCreating_AppliesAddressConfiguration()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Address));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    [Fact]
    public void CustomerDbContext_ModelCreating_AppliesOrderSummaryConfiguration()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CustomerDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(OrderSummary));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    #endregion

    #region Customer CRUD Operations

    [Fact]
    public void CustomerDbContext_Customer_Add_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var customerId = Guid.NewGuid();

        using (var context = new CustomerDbContext(options))
        {
            context.Customers.Add(new Customer
            {
                Id = customerId,
                TenantId = Guid.NewGuid(),
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe"
            });
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var customer = context.Customers.Find(customerId);
            Assert.NotNull(customer);
            Assert.Equal("john.doe@example.com", customer.Email);
            Assert.Equal("John", customer.FirstName);
            Assert.Equal("Doe", customer.LastName);
        }
    }

    [Fact]
    public void CustomerDbContext_Customer_Update_ShouldPersistChanges()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var customerId = Guid.NewGuid();

        using (var context = new CustomerDbContext(options))
        {
            context.Customers.Add(new Customer
            {
                Id = customerId,
                TenantId = Guid.NewGuid(),
                Email = "original@example.com",
                FirstName = "Original",
                LastName = "Name"
            });
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var customer = context.Customers.Find(customerId);
            Assert.NotNull(customer);
            customer.FirstName = "Updated";
            customer.LastName = "Customer";
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var customer = context.Customers.Find(customerId);
            Assert.NotNull(customer);
            Assert.Equal("Updated", customer.FirstName);
            Assert.Equal("Customer", customer.LastName);
        }
    }

    [Fact]
    public void CustomerDbContext_Customer_Delete_ShouldRemove()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var customerId = Guid.NewGuid();

        using (var context = new CustomerDbContext(options))
        {
            context.Customers.Add(new Customer
            {
                Id = customerId,
                TenantId = Guid.NewGuid(),
                Email = "delete@example.com",
                FirstName = "Delete",
                LastName = "Me"
            });
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var customer = context.Customers.Find(customerId);
            Assert.NotNull(customer);
            context.Customers.Remove(customer);
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var customer = context.Customers.Find(customerId);
            Assert.Null(customer);
        }
    }

    [Fact]
    public void CustomerDbContext_Customer_QueryByTenantId_ShouldFilter()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using (var context = new CustomerDbContext(options))
        {
            context.Customers.AddRange(
                new Customer
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantA,
                    Email = "tenant-a-1@example.com",
                    FirstName = "Tenant",
                    LastName = "A1"
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantB,
                    Email = "tenant-b@example.com",
                    FirstName = "Tenant",
                    LastName = "B"
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantA,
                    Email = "tenant-a-2@example.com",
                    FirstName = "Tenant",
                    LastName = "A2"
                });
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var tenantACustomers = context.Customers.Where(c => c.TenantId == tenantA).ToList();
            var tenantBCustomers = context.Customers.Where(c => c.TenantId == tenantB).ToList();

            Assert.Equal(2, tenantACustomers.Count);
            Assert.Single(tenantBCustomers);
            Assert.All(tenantACustomers, c => Assert.Equal(tenantA, c.TenantId));
            Assert.All(tenantBCustomers, c => Assert.Equal(tenantB, c.TenantId));
        }
    }

    #endregion

    #region Address CRUD Operations

    [Fact]
    public void CustomerDbContext_Address_Add_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var addressId = Guid.NewGuid();

        using (var context = new CustomerDbContext(options))
        {
            context.Addresses.Add(new Address
            {
                Id = addressId,
                CustomerId = Guid.NewGuid(),
                Street = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "US"
            });
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var address = context.Addresses.Find(addressId);
            Assert.NotNull(address);
            Assert.Equal("123 Main St", address.Street);
            Assert.Equal("New York", address.City);
        }
    }

    [Fact]
    public void CustomerDbContext_Address_WithCustomer_CanBeAddedTogether()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var customerId = Guid.NewGuid();
        var addressId = Guid.NewGuid();

        using (var context = new CustomerDbContext(options))
        {
            var customer = new Customer
            {
                Id = customerId,
                TenantId = Guid.NewGuid(),
                Email = "withaddress@example.com",
                FirstName = "With",
                LastName = "Address"
            };

            var address = new Address
            {
                Id = addressId,
                CustomerId = customerId,
                Street = "456 Oak Ave",
                City = "Los Angeles",
                PostalCode = "90001",
                Country = "US"
            };

            context.Customers.Add(customer);
            context.Addresses.Add(address);
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var address = context.Addresses.Find(addressId);
            Assert.NotNull(address);
            Assert.Equal(customerId, address.CustomerId);
        }
    }

    #endregion

    #region OrderSummary CRUD Operations

    [Fact]
    public void CustomerDbContext_OrderSummary_Add_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var orderSummaryId = Guid.NewGuid();

        using (var context = new CustomerDbContext(options))
        {
            context.OrderSummaries.Add(new OrderSummary
            {
                Id = orderSummaryId,
                TenantId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                TotalAmount = 199.99m,
                Status = "Completed"
            });
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var orderSummary = context.OrderSummaries.Find(orderSummaryId);
            Assert.NotNull(orderSummary);
            Assert.Equal(199.99m, orderSummary.TotalAmount);
            Assert.Equal("Completed", orderSummary.Status);
        }
    }

    [Fact]
    public void CustomerDbContext_OrderSummary_QueryByCustomerId_ShouldFilter()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var customerId = Guid.NewGuid();

        using (var context = new CustomerDbContext(options))
        {
            context.OrderSummaries.AddRange(
                new OrderSummary
                {
                    Id = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    OrderId = Guid.NewGuid(),
                    CustomerId = customerId,
                    TotalAmount = 50.00m,
                    Status = "Completed"
                },
                new OrderSummary
                {
                    Id = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    OrderId = Guid.NewGuid(),
                    CustomerId = customerId,
                    TotalAmount = 75.00m,
                    Status = "Pending"
                },
                new OrderSummary
                {
                    Id = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    OrderId = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    TotalAmount = 100.00m,
                    Status = "Completed"
                });
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var customerOrders = context.OrderSummaries.Where(o => o.CustomerId == customerId).ToList();

            Assert.Equal(2, customerOrders.Count);
            Assert.All(customerOrders, o => Assert.Equal(customerId, o.CustomerId));
        }
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void CustomerDbContext_FullCustomerGraph_CanBePersisted()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var customerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        using (var context = new CustomerDbContext(options))
        {
            var customer = new Customer
            {
                Id = customerId,
                TenantId = tenantId,
                Email = "full.graph@example.com",
                FirstName = "Full",
                LastName = "Graph",
                PhoneNumber = "+1-555-123-4567"
            };

            var shippingAddress = new Address
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
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
                CustomerId = customerId,
                Type = AddressType.Billing,
                Street = "200 Billing Blvd",
                City = "Boston",
                State = "MA",
                PostalCode = "02101",
                Country = "US"
            };

            var orderSummary = new OrderSummary
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                OrderId = Guid.NewGuid(),
                CustomerId = customerId,
                TotalAmount = 299.99m,
                Status = "Delivered"
            };

            context.Customers.Add(customer);
            context.Addresses.Add(shippingAddress);
            context.Addresses.Add(billingAddress);
            context.OrderSummaries.Add(orderSummary);
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            Assert.NotNull(context.Customers.Find(customerId));
            Assert.Equal(2, context.Addresses.Count(a => a.CustomerId == customerId));
            Assert.Single(context.OrderSummaries.Where(o => o.CustomerId == customerId));
        }
    }

    [Fact]
    public void CustomerDbContext_MultipleContexts_Isolated()
    {
        var optionsA = CreateInMemoryOptions("Customer-DB-A");
        var optionsB = CreateInMemoryOptions("Customer-DB-B");

        using (var contextA = new CustomerDbContext(optionsA))
        {
            contextA.Customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Email = "db-a@example.com",
                FirstName = "DB",
                LastName = "A"
            });
            contextA.SaveChanges();
        }

        using (var contextB = new CustomerDbContext(optionsB))
        {
            contextB.Customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Email = "db-b@example.com",
                FirstName = "DB",
                LastName = "B"
            });
            contextB.SaveChanges();
        }

        using (var contextA = new CustomerDbContext(optionsA))
        {
            using (var contextB = new CustomerDbContext(optionsB))
            {
                Assert.Single(contextA.Customers);
                Assert.Single(contextB.Customers);

                var customerA = contextA.Customers.First();
                var customerB = contextB.Customers.First();

                Assert.Equal("db-a@example.com", customerA.Email);
                Assert.Equal("db-b@example.com", customerB.Email);
            }
        }
    }

    #endregion

    #region Change Tracking

    [Fact]
    public void CustomerDbContext_SaveChanges_ReturnsAffectedCount()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CustomerDbContext(options);
        context.Customers.Add(new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "tracked@example.com",
            FirstName = "Tracked",
            LastName = "Customer"
        });

        var affected = context.SaveChanges();

        Assert.Equal(1, affected);
    }

    [Fact]
    public void CustomerDbContext_ChangeTracker_TracksAddedEntity()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CustomerDbContext(options);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "added@example.com",
            FirstName = "Added",
            LastName = "Customer"
        };

        context.Customers.Add(customer);

        var entry = context.Entry(customer);
        Assert.Equal(EntityState.Added, entry.State);
    }

    [Fact]
    public void CustomerDbContext_ChangeTracker_TracksModifiedEntity()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var customerId = Guid.NewGuid();

        using (var context = new CustomerDbContext(options))
        {
            context.Customers.Add(new Customer
            {
                Id = customerId,
                TenantId = Guid.NewGuid(),
                Email = "original@example.com",
                FirstName = "Original",
                LastName = "Customer"
            });
            context.SaveChanges();
        }

        using (var context = new CustomerDbContext(options))
        {
            var customer = context.Customers.Find(customerId);
            Assert.NotNull(customer);
            customer.FirstName = "Modified";

            var entry = context.Entry(customer);
            Assert.Equal(EntityState.Modified, entry.State);
        }
    }

    #endregion
}
