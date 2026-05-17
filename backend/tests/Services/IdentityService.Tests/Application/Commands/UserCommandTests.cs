using IdentityService.Application.Commands;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityService.Tests.Application.Commands;

/// <summary>
/// Unit tests for user command handlers.
/// </summary>
public class UserCommandTests
{
    private static IdentityDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new IdentityDbContext(options);
    }

    /// <summary>
    /// Tests that <see cref="CreateUserCommandHandler"/> creates a user with a hashed password.
    /// </summary>
    [Fact]
    public async Task CreateUserCommand_CreatesUserWithHashedPassword()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Subdomain = "test" };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var handler = new CreateUserCommandHandler(db);
        var result = await handler.Handle(new CreateUserCommand("newuser@test.com", "plain-password", "New User", tenant.Id, UserRole.Admin), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("newuser@test.com", result.Email);
        Assert.Equal("New User", result.DisplayName);
        Assert.Equal(tenant.Id, result.TenantId);
        Assert.Equal(UserRole.Admin, result.Role);
        Assert.NotEqual("plain-password", result.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("plain-password", result.PasswordHash));

        var savedUser = await db.Users.FindAsync(result.Id);
        Assert.NotNull(savedUser);
        Assert.Equal(result.Email, savedUser.Email);
    }

    /// <summary>
    /// Tests that <see cref="DeleteUserCommandHandler"/> removes an existing user.
    /// </summary>
    [Fact]
    public async Task DeleteUserCommand_ExistingUser_ReturnsTrueAndRemovesUser()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Subdomain = "test" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "delete@test.com",
            PasswordHash = "hash",
            DisplayName = "Delete Me",
            TenantId = tenant.Id
        };

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new DeleteUserCommandHandler(db);
        var result = await handler.Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

        Assert.True(result);
        var deletedUser = await db.Users.FindAsync(user.Id);
        Assert.Null(deletedUser);
    }

    /// <summary>
    /// Tests that <see cref="DeleteUserCommandHandler"/> returns false for a non-existent user.
    /// </summary>
    [Fact]
    public async Task DeleteUserCommand_NonExistingUser_ReturnsFalse()
    {
        using var db = CreateInMemoryContext();
        var handler = new DeleteUserCommandHandler(db);
        var result = await handler.Handle(new DeleteUserCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result);
    }

    /// <summary>
    /// Tests that <see cref="RegisterCommandHandler"/> creates a new user and returns the result.
    /// </summary>
    [Fact]
    public async Task RegisterCommand_NewEmail_CreatesUser()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Subdomain = "test" };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var handler = new RegisterCommandHandler(db);
        var result = await handler.Handle(new RegisterCommand("register@test.com", "password123", "Registered User", tenant.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("register@test.com", result.Email);
        Assert.NotEqual(Guid.Empty, result.UserId);

        var savedUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "register@test.com");
        Assert.NotNull(savedUser);
        Assert.Equal("Registered User", savedUser.DisplayName);
        Assert.Equal(UserRole.Member, savedUser.Role);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", savedUser.PasswordHash));
    }

    /// <summary>
    /// Tests that <see cref="RegisterCommandHandler"/> throws <see cref="InvalidOperationException"/> for a duplicate email.
    /// </summary>
    [Fact]
    public async Task RegisterCommand_DuplicateEmail_ThrowsInvalidOperationException()
    {
        using var db = CreateInMemoryContext();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", Subdomain = "test" };
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "duplicate@test.com",
            PasswordHash = "hash",
            DisplayName = "Existing User",
            TenantId = tenant.Id
        };

        db.Tenants.Add(tenant);
        db.Users.Add(existingUser);
        await db.SaveChangesAsync();

        var handler = new RegisterCommandHandler(db);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new RegisterCommand("duplicate@test.com", "password", "New User", tenant.Id), CancellationToken.None));

        Assert.Equal("Email already registered", exception.Message);
    }
}
