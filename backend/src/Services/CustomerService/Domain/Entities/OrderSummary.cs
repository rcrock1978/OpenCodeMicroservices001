using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerService.Domain.Entities;

/// <summary>
/// Represents a denormalized order summary for a customer's history.
/// </summary>
public class OrderSummary
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
    /// Gets or sets the order identifier (from OrderService).
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Navigation property for the customer.
    /// </summary>
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the order total amount.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the order status.
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Gets or sets when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Entity framework configuration for <see cref="OrderSummary"/>.
/// </summary>
public class OrderSummaryConfiguration : IEntityTypeConfiguration<OrderSummary>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderSummary> builder)
    {
        builder.HasKey(o => o.Id);
        builder.HasIndex(o => new { o.TenantId, o.OrderId }).IsUnique();
        builder.HasIndex(o => new { o.TenantId, o.CustomerId });
        builder.HasOne(o => o.Customer)
               .WithMany()
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
