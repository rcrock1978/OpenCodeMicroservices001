using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Domain.Entities;

/// <summary>
/// Represents a stock movement (in, out, adjustment) for an inventory item.
/// </summary>
public class StockMovement
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
    /// Gets or sets the inventory item identifier.
    /// </summary>
    public Guid StockItemId { get; set; }

    /// <summary>
    /// Navigation property for the inventory item.
    /// </summary>
    public StockItem StockItem { get; set; } = null!;

    /// <summary>
    /// Gets or sets the movement type.
    /// </summary>
    public StockMovementType Type { get; set; }

    /// <summary>
    /// Gets or sets the quantity moved.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the reference (order id, adjustment reason, etc.).
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// Gets or sets when the movement was recorded.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Defines the possible stock movement types.
/// </summary>
public enum StockMovementType
{
    Inbound,
    Outbound,
    Adjustment,
    Reservation,
    Release
}

/// <summary>
/// Entity framework configuration for <see cref="StockMovement"/>.
/// </summary>
public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.TenantId, m.StockItemId });
        builder.HasIndex(m => m.CreatedAt);
        builder.HasOne(m => m.StockItem)
               .WithMany()
               .HasForeignKey(m => m.StockItemId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
