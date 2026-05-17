using IdentityService.Application.Queries;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityService.Tests.Application.Queries;

/// <summary>
/// Unit tests for tenant query handlers.
/// </summary>
public class TenantQueryTests
{
    private static IdentityDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new IdentityDbContext(options);
    }

    /// <summary>
    /// Tests that <see cref="GetTenantsQueryHandler"/> returns all seeded tenants.
    /// </summary>
    [Fact]
    public async Task GetTenantsQuery_ReturnsAllTenants()
    {
        using var db = CreateInMemoryContext();
        var tenant1 = new Tenant { Id = Guid.NewGuid(), Name = "Tenant A", Subdomain = "tenant-a" };
        var tenant2 = new Tenant { Id = Guid.NewGuid(), Name = "Tenant B", Subdomain = "tenant-b" };
        db.Tenants.AddRange(tenant1, tenant2);
        await db.SaveChangesAsync();

        var handler = new GetTenantsQueryHandler(db);
        var result = await handler.Handle(new GetTenantsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Name == "Tenant A");
        Assert.Contains(result, t => t.Name == "Tenant B");
    }

    /// <summary>
    /// Tests that <see cref="GetTenantsQueryHandler"/> returns an empty list when no tenants exist.
    /// </summary>
    [Fact]
    public async Task GetTenantsQuery_NoTenants_ReturnsEmptyList()
    {
        using var db = CreateInMemoryContext();
        var handler = new GetTenantsQueryHandler(db);
        var result = await handler.Handle(new GetTenantsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that <see cref="GetTenantByIdQueryHandler"/> returns the correct tenant when found.
    /// </summary>
    [Fact]
    public async Task GetTenantByIdQuery_ExistingId_ReturnsTenant()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Specific Tenant", Subdomain = "specific" };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var handler = new GetTenantByIdQueryHandler(db);
        var result = await handler.Handle(new GetTenantByIdQuery(tenant.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(tenant.Id, result.Id);
        Assert.Equal("Specific Tenant", result.Name);
    }

    /// <summary>
    /// Tests that <see cref="GetTenantByIdQueryHandler"/> returns null for a non-existent tenant.
    /// </summary>
    [Fact]
    public async Task GetTenantByIdQuery_NonExistingId_ReturnsNull()
    {
        using var db = CreateInMemoryContext();
        var handler = new GetTenantByIdQueryHandler(db);
        var result = await handler.Handle(new GetTenantByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
