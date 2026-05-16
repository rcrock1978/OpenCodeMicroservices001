using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Represents a payment intent (simulated, no real provider integration).
/// </summary>
public class PaymentIntent
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the amount in smallest currency unit.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the idempotency key.
    /// </summary>
    public required string IdempotencyKey { get; set; }

    /// <summary>
    /// Gets or sets the current status.
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Gets or sets the payment method snapshot.
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the failure reason if payment failed.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Gets or sets when the intent was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the intent was captured.
    /// </summary>
    public DateTime? CapturedAt { get; set; }
}

/// <summary>
/// Defines the possible payment statuses.
/// </summary>
public enum PaymentStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed,
    Cancelled,
    Refunded
}

/// <summary>
/// Entity framework configuration for <see cref="PaymentIntent"/>.
/// </summary>
public class PaymentIntentConfiguration : IEntityTypeConfiguration<PaymentIntent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PaymentIntent> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.TenantId, p.IdempotencyKey }).IsUnique();
        builder.HasIndex(p => p.OrderId);
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.IdempotencyKey).HasMaxLength(100).IsRequired();
    }
}

/// <summary>
/// Represents a stored payment method for a customer.
/// </summary>
public class PaymentMethod
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the method type.
    /// </summary>
    public PaymentMethodType Type { get; set; } = PaymentMethodType.Card;

    /// <summary>
    /// Gets or sets the last four digits.
    /// </summary>
    public string? LastFour { get; set; }

    /// <summary>
    /// Gets or sets the brand (e.g., Visa, Mastercard).
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Gets or sets the expiration month.
    /// </summary>
    public int? ExpMonth { get; set; }

    /// <summary>
    /// Gets or sets the expiration year.
    /// </summary>
    public int? ExpYear { get; set; }

    /// <summary>
    /// Gets or sets whether this is the default payment method.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets when the method was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Defines the possible payment method types.
/// </summary>
public enum PaymentMethodType
{
    Card,
    BankTransfer,
    Wallet
}

/// <summary>
/// Entity framework configuration for <see cref="PaymentMethod"/>.
/// </summary>
public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.TenantId, p.CustomerId });
        builder.Property(p => p.LastFour).HasMaxLength(4);
        builder.Property(p => p.Brand).HasMaxLength(50);
    }
}
