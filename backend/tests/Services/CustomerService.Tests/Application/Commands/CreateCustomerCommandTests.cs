using CustomerService.Application.Commands;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using CustomerService.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging.IntegrationEvents;
using Xunit;

namespace CustomerService.Tests.Application.Commands;

/// <summary>
/// Unit tests for <see cref="CreateCustomerCommandHandler"/>.
/// </summary>
public class CreateCustomerCommandTests
{
    private static CustomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CustomerDbContext(options);
    }

    [Fact]
    public async Task Handle_CreatesCustomerAndReturnsResponse()
    {
        using var context = CreateInMemoryContext();
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CreateCustomerCommandHandler(context, fakePublisher);

        var command = new CreateCustomerCommand(
            TenantId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Email: "new@example.com",
            FirstName: "New",
            LastName: "Customer",
            PhoneNumber: "+1-555-0199");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(command.TenantId, result.TenantId);
        Assert.Equal(command.UserId, result.UserId);
        Assert.Equal(command.Email, result.Email);
        Assert.Equal(command.FirstName, result.FirstName);
        Assert.Equal(command.LastName, result.LastName);
        Assert.Equal(command.PhoneNumber, result.PhoneNumber);
        Assert.NotNull(result.Addresses);
        Assert.Empty(result.Addresses);

        var dbCustomer = await context.Customers.FindAsync(result.Id);
        Assert.NotNull(dbCustomer);
        Assert.Equal(command.Email, dbCustomer.Email);
    }

    [Fact]
    public async Task Handle_WithoutUserId_CreatesCustomerWithNullUserId()
    {
        using var context = CreateInMemoryContext();
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CreateCustomerCommandHandler(context, fakePublisher);

        var command = new CreateCustomerCommand(
            TenantId: Guid.NewGuid(),
            UserId: null,
            Email: "nouser@example.com",
            FirstName: "No",
            LastName: "User",
            PhoneNumber: null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Null(result.UserId);
        Assert.Null(result.PhoneNumber);
    }

    [Fact]
    public async Task Handle_PublishesCustomerCreatedIntegrationEvent()
    {
        using var context = CreateInMemoryContext();
        var fakePublisher = new FakePublishEndpoint();
        var handler = new CreateCustomerCommandHandler(context, fakePublisher);

        var tenantId = Guid.NewGuid();
        var command = new CreateCustomerCommand(
            TenantId: tenantId,
            UserId: Guid.NewGuid(),
            Email: "event@example.com",
            FirstName: "Event",
            LastName: "Publisher",
            PhoneNumber: "+1-555-0200");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Single(fakePublisher.PublishedMessages);
        var publishedEvent = Assert.IsType<CustomerCreatedIntegrationEvent>(fakePublisher.PublishedMessages[0]);
        Assert.Equal(result.Id, publishedEvent.CustomerId);
        Assert.Equal(tenantId, publishedEvent.TenantId);
        Assert.Equal(command.Email, publishedEvent.Email);
        Assert.Equal($"{command.FirstName} {command.LastName}", publishedEvent.FullName);
    }
}
