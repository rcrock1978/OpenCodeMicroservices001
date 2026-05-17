using CustomerService.Application.Queries;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerService.Tests.Application.Queries;

/// <summary>
/// Unit tests for <see cref="GetCustomersByTenantQueryHandler"/>.
/// </summary>
public class GetCustomersByTenantQueryTests
{
    private static CustomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CustomerDbContext(options);
    }

    [Fact]
    public async Task Handle_WithNoMatchingTenant_ReturnsEmptyList()
    {
        using var context = CreateInMemoryContext();
        var tenantId = Guid.NewGuid();

        context.Customers.Add(new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "other@example.com",
            FirstName = "Other",
            LastName = "Tenant"
        });
        await context.SaveChangesAsync();

        var handler = new GetCustomersByTenantQueryHandler(context);
        var result = await handler.Handle(new GetCustomersByTenantQuery(tenantId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithMatchingTenant_ReturnsOnlyMatchingCustomers()
    {
        using var context = CreateInMemoryContext();
        var tenantId = Guid.NewGuid();

        context.Customers.AddRange(
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = "match1@example.com",
                FirstName = "Match",
                LastName = "One"
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = "match2@example.com",
                FirstName = "Match",
                LastName = "Two"
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Email = "other@example.com",
                FirstName = "Other",
                LastName = "Tenant"
            });
        await context.SaveChangesAsync();

        var handler = new GetCustomersByTenantQueryHandler(context);
        var result = await handler.Handle(new GetCustomersByTenantQuery(tenantId), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(tenantId, c.TenantId));
    }
}
