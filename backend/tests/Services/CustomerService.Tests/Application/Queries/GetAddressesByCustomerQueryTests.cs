using CustomerService.Application.Queries;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetAddressesByCustomerQueryHandler"/>.
/// </summary>
public class GetAddressesByCustomerQueryTests
{
    private static CustomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CustomerDbContext(options);
    }

    [Fact]
    public async Task Handle_WithNoAddresses_ReturnsEmptyList()
    {
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();

        var handler = new GetAddressesByCustomerQueryHandler(context);
        var result = await handler.Handle(new GetAddressesByCustomerQuery(customerId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithMatchingAddresses_ReturnsMappedResponses()
    {
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();

        context.Addresses.AddRange(
            new Address
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Type = AddressType.Shipping,
                Street = "100 Shipping Ln",
                City = "Boston",
                State = "MA",
                PostalCode = "02101",
                Country = "US",
                IsDefault = true
            },
            new Address
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Type = AddressType.Billing,
                Street = "200 Billing Blvd",
                City = "Boston",
                State = "MA",
                PostalCode = "02101",
                Country = "US",
                IsDefault = false
            },
            new Address
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Type = AddressType.Shipping,
                Street = "300 Other St",
                City = "Chicago",
                State = "IL",
                PostalCode = "60601",
                Country = "US",
                IsDefault = true
            });
        await context.SaveChangesAsync();

        var handler = new GetAddressesByCustomerQueryHandler(context);
        var result = await handler.Handle(new GetAddressesByCustomerQuery(customerId), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal(customerId, a.CustomerId));
    }
}
