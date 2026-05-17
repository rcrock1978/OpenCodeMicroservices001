using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence;

/// <summary>
/// Provides seed data for the Identity Service database.
/// </summary>
public static class IdentityDbContextSeed
{
    /// <summary>
    /// Seeds the database with initial tenants and users.
    /// </summary>
    public static async Task SeedAsync(IdentityDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        await SeedTenantsAsync(context);
        await SeedUsersAsync(context);
    }

    private static async Task SeedTenantsAsync(IdentityDbContext context)
    {
        if (await context.Tenants.AnyAsync())
            return;

        var tenants = new List<Tenant>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Acme Corporation",
                Subdomain = "acme",
                SubscriptionPlanId = "plan_enterprise",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "GlobalMart Inc",
                Subdomain = "globalmart",
                SubscriptionPlanId = "plan_business",
                IsActive = true,
                CreatedAt = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "TechStart Labs",
                Subdomain = "techstart",
                SubscriptionPlanId = "plan_starter",
                IsActive = true,
                CreatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Demo Tenant",
                Subdomain = "demo",
                SubscriptionPlanId = "plan_free",
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        await context.Tenants.AddRangeAsync(tenants);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(IdentityDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        // All seeded users share the same password for demo purposes.
        // Password: Password123!
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        var users = new List<User>
        {
            // Acme Corporation users
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Email = "owner@acme.com",
                PasswordHash = defaultPasswordHash,
                DisplayName = "John Acme",
                TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Role = UserRole.Owner,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab"),
                Email = "admin@acme.com",
                PasswordHash = defaultPasswordHash,
                DisplayName = "Jane Admin",
                TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac"),
                Email = "member@acme.com",
                PasswordHash = defaultPasswordHash,
                DisplayName = "Bob Member",
                TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Role = UserRole.Member,
                IsActive = true,
                CreatedAt = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc)
            },

            // GlobalMart Inc users
            new()
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Email = "owner@globalmart.com",
                PasswordHash = defaultPasswordHash,
                DisplayName = "Sarah Global",
                TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Role = UserRole.Owner,
                IsActive = true,
                CreatedAt = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbc"),
                Email = "manager@globalmart.com",
                PasswordHash = defaultPasswordHash,
                DisplayName = "Mike Manager",
                TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // TechStart Labs users
            new()
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Email = "founder@techstart.com",
                PasswordHash = defaultPasswordHash,
                DisplayName = "Alex Founder",
                TenantId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Role = UserRole.Owner,
                IsActive = true,
                CreatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccd"),
                Email = "dev@techstart.com",
                PasswordHash = defaultPasswordHash,
                DisplayName = "Dev Developer",
                TenantId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Role = UserRole.Member,
                IsActive = true,
                CreatedAt = new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccce"),
                Email = "inactive@techstart.com",
                PasswordHash = defaultPasswordHash,
                DisplayName = "Inactive User",
                TenantId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Role = UserRole.Member,
                IsActive = false,
                CreatedAt = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc)
            },

            // Demo Tenant users
            new()
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Email = "demo@demo.com",
                PasswordHash = defaultPasswordHash,
                DisplayName = "Demo User",
                TenantId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Role = UserRole.Owner,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        // Generate 1000 additional random users across all tenants
        var tenantIds = new[]
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Guid.Parse("44444444-4444-4444-4444-444444444444")
        };

        var firstNames = new[] { "James", "Mary", "Robert", "Patricia", "John", "Jennifer", "Michael", "Linda", "David", "Elizabeth", "William", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Lisa", "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson" };
        var roles = new[] { UserRole.Member, UserRole.Member, UserRole.Member, UserRole.Admin };

        var random = new Random(42); // seeded for reproducibility
        for (int i = 0; i < 1000; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var tenantId = tenantIds[random.Next(tenantIds.Length)];
            var role = roles[random.Next(roles.Length)];
            var displayName = $"{firstName} {lastName} {i:D4}";
            var email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}.{i:D4}@example.com";

            users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = defaultPasswordHash,
                DisplayName = displayName,
                TenantId = tenantId,
                Role = role,
                IsActive = random.NextDouble() > 0.1, // ~90% active
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 365))
            });
        }

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
