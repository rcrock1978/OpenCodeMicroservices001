using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingService.Domain.Entities;

/// <summary>
/// Represents an invoice for a tenant's subscription.
/// </summary>
public class Invoice
{
    /// <summary>
    /// Gets or sets the unique identifier for the invoice.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the subscription identifier.
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the invoice amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the invoice period start.
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Gets or sets the invoice period end.
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Gets or sets the invoice status.
    /// </summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

    /// <summary>
    /// Gets or sets when the invoice was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the invoice was paid.
    /// </summary>
    public DateTime? PaidAt { get; set; }
}

/// <summary>
/// Defines the possible invoice statuses.
/// </summary>
public enum InvoiceStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}

/// <summary>
/// Entity framework configuration for <see cref="Invoice"/>.
/// </summary>
public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Currency).HasMaxLength(3).IsRequired();
    }
}
