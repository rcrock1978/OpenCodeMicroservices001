using CustomerService.Application.Commands;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="CreateAddressCommandHandler"/>.
/// </summary>
public class CreateAddressCommandTests
{
    private static CustomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CustomerDbContext(options);
    }

    [Fact]
    public async Task Handle_CreatesAddressAndReturnsResponse()
    {
        using var context = CreateInMemoryContext();
        var handler = new CreateAddressCommandHandler(context);

        var customerId = Guid.NewGuid();
        var command = new CreateAddressCommand(
            CustomerId: customerId,
            Type: AddressType.Shipping,
            Street: "789 Pine Rd",
            City: "Austin",
            State: "TX",
            PostalCode: "78701",
            Country: "US",
            IsDefault: true);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(command.Type, result.Type);
        Assert.Equal(command.Street, result.Street);
        Assert.Equal(command.City, result.City);
        Assert.Equal(command.State, result.State);
        Assert.Equal(command.PostalCode, result.PostalCode);
        Assert.Equal(command.Country, result.Country);
        Assert.Equal(command.IsDefault, result.IsDefault);

        var dbAddress = await context.Addresses.FindAsync(result.Id);
        Assert.NotNull(dbAddress);
        Assert.Equal(command.Street, dbAddress.Street);
    }

    [Fact]
    public async Task Handle_WithNullState_CreatesAddressSuccessfully()
    {
        using var context = CreateInMemoryContext();
        var handler = new CreateAddressCommandHandler(context);

        var command = new CreateAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Billing,
            Street: "999 No State Ln",
            City: "Paris",
            State: null,
            PostalCode: "75001",
            Country: "FR",
            IsDefault: false);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Null(result.State);

        var dbAddress = await context.Addresses.FindAsync(result.Id);
        Assert.NotNull(dbAddress);
        Assert.Null(dbAddress.State);
    }
}
