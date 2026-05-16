using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Domain.Entities;

/// <summary>
/// Represents a tenant in the multi-tenant SaaS platform.
/// </summary>
public class Tenant
{
    /// <summary>
    /// Gets or sets the unique identifier for the tenant.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the tenant.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the unique subdomain for the tenant.
    /// </summary>
    public required string Subdomain { get; set; }

    /// <summary>
    /// Gets or sets the subscription plan identifier.
    /// </summary>
    public string? SubscriptionPlanId { get; set; }

    /// <summary>
    /// Gets or sets when the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether the tenant is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for users belonging to this tenant.
    /// </summary>
    public ICollection<User> Users { get; set; } = [];
}

/// <summary>
/// Entity framework configuration for <see cref="Tenant"/>.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.Subdomain).IsUnique();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Subdomain).HasMaxLength(100).IsRequired();
    }
}
