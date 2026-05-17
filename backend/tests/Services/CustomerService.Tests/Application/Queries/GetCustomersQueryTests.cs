using CustomerService.Application.Queries;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetCustomersQueryHandler"/>.
/// </summary>
public class GetCustomersQueryTests
{
    private static CustomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CustomerDbContext(options);
    }

    [Fact]
    public async Task Handle_WithNoCustomers_ReturnsEmptyList()
    {
        using var context = CreateInMemoryContext();
        var handler = new GetCustomersQueryHandler(context);

        var result = await handler.Handle(new GetCustomersQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithCustomers_ReturnsMappedResponses()
    {
        using var context = CreateInMemoryContext();
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "alice@example.com",
            FirstName = "Alice",
            LastName = "Smith",
            PhoneNumber = "+1-555-0100"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var handler = new GetCustomersQueryHandler(context);
        var result = await handler.Handle(new GetCustomersQuery(), CancellationToken.None);

        Assert.Single(result);
        var response = result[0];
        Assert.Equal(customer.Id, response.Id);
        Assert.Equal(customer.TenantId, response.TenantId);
        Assert.Equal(customer.Email, response.Email);
        Assert.Equal(customer.FirstName, response.FirstName);
        Assert.Equal(customer.LastName, response.LastName);
        Assert.Equal(customer.PhoneNumber, response.PhoneNumber);
        Assert.NotNull(response.Addresses);
    }

    [Fact]
    public async Task Handle_WithCustomersAndAddresses_ReturnsMappedAddresses()
    {
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            TenantId = Guid.NewGuid(),
            Email = "bob@example.com",
            FirstName = "Bob",
            LastName = "Jones"
        };

        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Type = AddressType.Shipping,
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "US",
            IsDefault = true
        };

        customer.Addresses.Add(address);
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var handler = new GetCustomersQueryHandler(context);
        var result = await handler.Handle(new GetCustomersQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Single(result[0].Addresses);
        var addr = result[0].Addresses[0];
        Assert.Equal(address.Id, addr.Id);
        Assert.Equal(address.Street, addr.Street);
        Assert.Equal(address.City, addr.City);
    }
}
