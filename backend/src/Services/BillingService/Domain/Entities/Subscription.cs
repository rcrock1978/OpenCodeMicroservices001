using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Domain.Entities;

/// <summary>
/// Represents a tenant's subscription to a plan.
/// </summary>
public class Subscription
{
    /// <summary>
    /// Gets or sets the unique identifier for the subscription.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the plan identifier.
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Navigation property for the plan.
    /// </summary>
    public Plan Plan { get; set; } = null!;

    /// <summary>
    /// Gets or sets the start date of the subscription.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the subscription.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the billing interval.
    /// </summary>
    public BillingInterval Interval { get; set; } = BillingInterval.Monthly;

    /// <summary>
    /// Gets or sets the current status of the subscription.
    /// </summary>
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
}

/// <summary>
/// Defines the possible billing intervals.
/// </summary>
public enum BillingInterval
{
    Monthly,
    Yearly
}

/// <summary>
/// Defines the possible subscription statuses.
/// </summary>
public enum SubscriptionStatus
{
    Active,
    Cancelled,
    PastDue,
    Expired
}

/// <summary>
/// Entity framework configuration for <see cref="Subscription"/>.
/// </summary>
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Plan)
               .WithMany()
               .HasForeignKey(s => s.PlanId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
