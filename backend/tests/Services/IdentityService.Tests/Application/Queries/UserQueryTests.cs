using IdentityService.Application.Queries;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IdentityService.Tests.Application.Queries;

/// <summary>
/// Unit tests for user query handlers.
/// </summary>
public class UserQueryTests
{
    private static IdentityDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new IdentityDbContext(options);
    }

    private static IConfiguration CreateFakeConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-key-for-testing-purposes-only!!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();
    }

    /// <summary>
    /// Tests that <see cref="GetUsersQueryHandler"/> returns all seeded users with tenants.
    /// </summary>
    [Fact]
    public async Task GetUsersQuery_ReturnsAllUsersWithTenants()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Subdomain = "test" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            PasswordHash = "hash",
            DisplayName = "Test User",
            TenantId = tenant.Id,
            Tenant = tenant
        };

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new GetUsersQueryHandler(db);
        var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("user@test.com", result[0].Email);
        Assert.NotNull(result[0].Tenant);
        Assert.Equal("Test Tenant", result[0].Tenant.Name);
    }

    /// <summary>
    /// Tests that <see cref="GetUsersQueryHandler"/> returns an empty list when no users exist.
    /// </summary>
    [Fact]
    public async Task GetUsersQuery_NoUsers_ReturnsEmptyList()
    {
        using var db = CreateInMemoryContext();
        var handler = new GetUsersQueryHandler(db);
        var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that <see cref="GetUserByIdQueryHandler"/> returns the correct user when found.
    /// </summary>
    [Fact]
    public async Task GetUserByIdQuery_ExistingId_ReturnsUser()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Subdomain = "test" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            PasswordHash = "hash",
            DisplayName = "Test User",
            TenantId = tenant.Id,
            Tenant = tenant
        };

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new GetUserByIdQueryHandler(db);
        var result = await handler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("user@test.com", result.Email);
        Assert.NotNull(result.Tenant);
    }

    /// <summary>
    /// Tests that <see cref="GetUserByIdQueryHandler"/> returns null for a non-existent user.
    /// </summary>
    [Fact]
    public async Task GetUserByIdQuery_NonExistingId_ReturnsNull()
    {
        using var db = CreateInMemoryContext();
        var handler = new GetUserByIdQueryHandler(db);
        var result = await handler.Handle(new GetUserByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    /// <summary>
    /// Tests that <see cref="LoginQueryHandler"/> returns a login response for valid credentials.
    /// </summary>
    [Fact]
    public async Task LoginQuery_ValidCredentials_ReturnsLoginResponse()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Subdomain = "test" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "login@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password"),
            DisplayName = "Login User",
            TenantId = tenant.Id,
            Tenant = tenant,
            IsActive = true
        };

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginQueryHandler(db, CreateFakeConfiguration());
        var result = await handler.Handle(new LoginQuery("login@test.com", "correct-password"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("login@test.com", result.Email);
        Assert.Equal("Login User", result.DisplayName);
        Assert.Equal(tenant.Id, result.TenantId);
        Assert.Equal(UserRole.Member.ToString(), result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    /// <summary>
    /// Tests that <see cref="LoginQueryHandler"/> returns null for an incorrect password.
    /// </summary>
    [Fact]
    public async Task LoginQuery_WrongPassword_ReturnsNull()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Subdomain = "test" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "login@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password"),
            DisplayName = "Login User",
            TenantId = tenant.Id,
            Tenant = tenant,
            IsActive = true
        };

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginQueryHandler(db, CreateFakeConfiguration());
        var result = await handler.Handle(new LoginQuery("login@test.com", "wrong-password"), CancellationToken.None);

        Assert.Null(result);
    }

    /// <summary>
    /// Tests that <see cref="LoginQueryHandler"/> returns null when the user does not exist.
    /// </summary>
    [Fact]
    public async Task LoginQuery_MissingUser_ReturnsNull()
    {
        using var db = CreateInMemoryContext();
        var handler = new LoginQueryHandler(db, CreateFakeConfiguration());
        var result = await handler.Handle(new LoginQuery("nobody@test.com", "any-password"), CancellationToken.None);

        Assert.Null(result);
    }

    /// <summary>
    /// Tests that <see cref="LoginQueryHandler"/> returns null when the user is inactive.
    /// </summary>
    [Fact]
    public async Task LoginQuery_InactiveUser_ReturnsNull()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Subdomain = "test" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            DisplayName = "Inactive User",
            TenantId = tenant.Id,
            Tenant = tenant,
            IsActive = false
        };

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new LoginQueryHandler(db, CreateFakeConfiguration());
        var result = await handler.Handle(new LoginQuery("inactive@test.com", "password"), CancellationToken.None);

        Assert.Null(result);
    }
}
