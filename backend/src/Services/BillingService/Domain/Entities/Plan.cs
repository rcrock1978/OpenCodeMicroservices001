using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Domain.Entities;

/// <summary>
/// Represents a subscription plan available in the SaaS platform.
/// </summary>
public class Plan
{
    /// <summary>
    /// Gets or sets the unique identifier for the plan.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the plan.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the plan.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the monthly price in the smallest currency unit (e.g., cents).
    /// </summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>
    /// Gets or sets the yearly price in the smallest currency unit.
    /// </summary>
    public decimal YearlyPrice { get; set; }

    /// <summary>
    /// Gets or sets the currency code (e.g., USD, EUR).
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the maximum number of users allowed.
    /// </summary>
    public int MaxUsers { get; set; }

    /// <summary>
    /// Gets or sets whether the plan is currently available.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Entity framework configuration for <see cref="Plan"/>.
/// </summary>
public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
    }
}
