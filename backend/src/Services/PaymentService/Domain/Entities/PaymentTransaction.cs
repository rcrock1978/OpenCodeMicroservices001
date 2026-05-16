using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Represents a payment transaction record.
/// </summary>
public class PaymentTransaction
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the payment intent identifier.
    /// </summary>
    public Guid PaymentIntentId { get; set; }

    /// <summary>
    /// Navigation property for the payment intent.
    /// </summary>
    public PaymentIntent PaymentIntent { get; set; } = null!;

    /// <summary>
    /// Gets or sets the transaction type.
    /// </summary>
    public PaymentTransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the transaction status.
    /// </summary>
    public PaymentTransactionStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the gateway response snapshot.
    /// </summary>
    public string? GatewayResponse { get; set; }

    /// <summary>
    /// Gets or sets when the transaction was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Defines the possible payment transaction types.
/// </summary>
public enum PaymentTransactionType
{
    Authorization,
    Capture,
    Refund,
    Void
}

/// <summary>
/// Defines the possible payment transaction statuses.
/// </summary>
public enum PaymentTransactionStatus
{
    Pending,
    Succeeded,
    Failed
}

/// <summary>
/// Entity framework configuration for <see cref="PaymentTransaction"/>.
/// </summary>
public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.PaymentIntentId);
        builder.HasOne(t => t.PaymentIntent)
               .WithMany()
               .HasForeignKey(t => t.PaymentIntentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
