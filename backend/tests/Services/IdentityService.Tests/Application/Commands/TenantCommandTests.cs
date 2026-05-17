using IdentityService.Application.Commands;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging.IntegrationEvents;
using Xunit;

namespace IdentityService.Tests.Application.Commands;

/// <summary>
/// Unit tests for tenant command handlers.
/// </summary>
public class TenantCommandTests
{
    private static IdentityDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new IdentityDbContext(options);
    }

    /// <summary>
    /// Tests that <see cref="CreateTenantCommandHandler"/> creates a tenant and publishes an integration event.
    /// </summary>
    [Fact]
    public async Task CreateTenantCommand_CreatesTenantAndPublishesEvent()
    {
        using var db = CreateInMemoryContext();
        var fakePublishEndpoint = new FakePublishEndpoint();
        var handler = new CreateTenantCommandHandler(db, fakePublishEndpoint);

        var result = await handler.Handle(new CreateTenantCommand("New Tenant", "new-tenant", "plan_basic"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New Tenant", result.Name);
        Assert.Equal("new-tenant", result.Subdomain);
        Assert.Equal("plan_basic", result.SubscriptionPlanId);
        Assert.NotEqual(Guid.Empty, result.Id);

        var savedTenant = await db.Tenants.FindAsync(result.Id);
        Assert.NotNull(savedTenant);

        Assert.Single(fakePublishEndpoint.PublishedMessages);
        var publishedEvent = fakePublishEndpoint.PublishedMessages[0] as TenantCreatedIntegrationEvent;
        Assert.NotNull(publishedEvent);
        Assert.Equal(result.Id, publishedEvent.TenantId);
        Assert.Equal("New Tenant", publishedEvent.Name);
        Assert.Equal("new-tenant", publishedEvent.Slug);
    }

    /// <summary>
    /// Tests that <see cref="CreateTenantCommandHandler"/> creates a tenant with a null subscription plan.
    /// </summary>
    [Fact]
    public async Task CreateTenantCommand_NullSubscriptionPlan_CreatesTenant()
    {
        using var db = CreateInMemoryContext();
        var fakePublishEndpoint = new FakePublishEndpoint();
        var handler = new CreateTenantCommandHandler(db, fakePublishEndpoint);

        var result = await handler.Handle(new CreateTenantCommand("Free Tenant", "free-tenant", null), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.SubscriptionPlanId);
        Assert.Single(fakePublishEndpoint.PublishedMessages);
    }

    /// <summary>
    /// Tests that <see cref="UpdateTenantCommandHandler"/> updates an existing tenant.
    /// </summary>
    [Fact]
    public async Task UpdateTenantCommand_ExistingTenant_UpdatesAndReturnsTenant()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Old Name", Subdomain = "old", IsActive = true };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var handler = new UpdateTenantCommandHandler(db);
        var result = await handler.Handle(new UpdateTenantCommand(tenant.Id, "Updated Name", false), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.False(result.IsActive);

        var updated = await db.Tenants.FindAsync(tenant.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.False(updated.IsActive);
    }

    /// <summary>
    /// Tests that <see cref="UpdateTenantCommandHandler"/> returns null for a non-existent tenant.
    /// </summary>
    [Fact]
    public async Task UpdateTenantCommand_NonExistingTenant_ReturnsNull()
    {
        using var db = CreateInMemoryContext();
        var handler = new UpdateTenantCommandHandler(db);
        var result = await handler.Handle(new UpdateTenantCommand(Guid.NewGuid(), "Name", true), CancellationToken.None);

        Assert.Null(result);
    }
}
