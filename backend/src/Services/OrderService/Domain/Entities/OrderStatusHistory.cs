using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderService.Domain.Entities;

/// <summary>
/// Represents a status change in an order's lifecycle.
/// </summary>
public class OrderStatusHistory
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Navigation property for the order.
    /// </summary>
    public Order Order { get; set; } = null!;

    /// <summary>
    /// Gets or sets the status at this point in time.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when the status changed.
    /// </summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the reason for the status change.
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Entity framework configuration for <see cref="OrderStatusHistory"/>.
/// </summary>
public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.HasIndex(h => new { h.OrderId, h.ChangedAt });
        builder.HasOne(h => h.Order)
               .WithMany()
               .HasForeignKey(h => h.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
