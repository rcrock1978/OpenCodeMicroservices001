using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Domain.Entities;

/// <summary>
/// Represents the inventory stock record for a product variant.
/// </summary>
public class StockItem
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
    /// Gets or sets the product variant identifier from the Catalog service.
    /// </summary>
    public Guid ProductVariantId { get; set; }

    /// <summary>
    /// Gets or sets the SKU.
    /// </summary>
    public required string Sku { get; set; }

    /// <summary>
    /// Gets or sets the quantity available for sale.
    /// </summary>
    public int QuantityAvailable { get; set; }

    /// <summary>
    /// Gets or sets the quantity reserved by pending orders.
    /// </summary>
    public int QuantityReserved { get; set; }

    /// <summary>
    /// Gets or sets the low stock threshold.
    /// </summary>
    public int LowStockThreshold { get; set; } = 10;

    /// <summary>
    /// Gets or sets when the record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Entity framework configuration for <see cref="StockItem"/>.
/// </summary>
public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.TenantId, s.ProductVariantId }).IsUnique();
        builder.HasIndex(s => s.Sku).IsUnique();
        builder.Property(s => s.Sku).HasMaxLength(100).IsRequired();
    }
}

/// <summary>
/// Represents a reservation of stock for an order.
/// </summary>
public class StockReservation
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
    /// Gets or sets the stock item identifier.
    /// </summary>
    public Guid StockItemId { get; set; }

    /// <summary>
    /// Gets or sets the reserved quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the reservation status.
    /// </summary>
    public ReservationStatus Status { get; set; } = ReservationStatus.Reserved;

    /// <summary>
    /// Gets or sets when the reservation was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the reservation expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Defines the possible reservation statuses.
/// </summary>
public enum ReservationStatus
{
    Reserved,
    Committed,
    Released,
    Expired
}

/// <summary>
/// Entity framework configuration for <see cref="StockReservation"/>.
/// </summary>
public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.TenantId, r.OrderId });
        builder.HasIndex(r => r.ExpiresAt);
    }
}
