using CustomerService.Application.Queries;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetCustomerByIdQueryHandler"/>.
/// </summary>
public class GetCustomerByIdQueryTests
{
    private static CustomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CustomerDbContext(options);
    }

    [Fact]
    public async Task Handle_WithExistingCustomer_ReturnsMappedResponse()
    {
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            TenantId = Guid.NewGuid(),
            Email = "found@example.com",
            FirstName = "Found",
            LastName = "Customer"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var handler = new GetCustomerByIdQueryHandler(context);
        var result = await handler.Handle(new GetCustomerByIdQuery(customerId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(customerId, result.Id);
        Assert.Equal(customer.Email, result.Email);
        Assert.Equal(customer.FirstName, result.FirstName);
        Assert.Equal(customer.LastName, result.LastName);
    }

    [Fact]
    public async Task Handle_WithExistingCustomerAndAddresses_ReturnsMappedAddresses()
    {
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            TenantId = Guid.NewGuid(),
            Email = "withaddr@example.com",
            FirstName = "With",
            LastName = "Address"
        };

        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Type = AddressType.Billing,
            Street = "456 Oak Ave",
            City = "Metropolis",
            State = "NY",
            PostalCode = "10001",
            Country = "US",
            IsDefault = false
        };

        customer.Addresses.Add(address);
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var handler = new GetCustomerByIdQueryHandler(context);
        var result = await handler.Handle(new GetCustomerByIdQuery(customerId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.Addresses);
        Assert.Equal(address.Id, result.Addresses[0].Id);
    }

    [Fact]
    public async Task Handle_WithNonExistingCustomer_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var handler = new GetCustomerByIdQueryHandler(context);
        var result = await handler.Handle(new GetCustomerByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
