using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderService.Domain.Entities;

/// <summary>
/// Represents a customer order.
/// </summary>
public class Order
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
    /// Gets or sets the order number (human-readable).
    /// </summary>
    public required string OrderNumber { get; set; }

    /// <summary>
    /// Gets or sets the current order status.
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Gets or sets the order subtotal.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Gets or sets the shipping cost.
    /// </summary>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Gets or sets the tax amount.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the order total.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the shipping address JSON.
    /// </summary>
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Gets or sets when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for order items.
    /// </summary>
    public ICollection<OrderItem> Items { get; set; } = [];
}

/// <summary>
/// Defines the possible order statuses.
/// </summary>
public enum OrderStatus
{
    Pending,
    InventoryReserved,
    PaymentInitiated,
    Paid,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

/// <summary>
/// Entity framework configuration for <see cref="Order"/>.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique();
        builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(o => o.Currency).HasMaxLength(3).IsRequired();
        builder.HasIndex(o => new { o.TenantId, o.CustomerId });
    }
}

/// <summary>
/// Represents a line item within an order.
/// </summary>
public class OrderItem
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
    /// Gets or sets the product identifier from the Catalog service.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product variant identifier.
    /// </summary>
    public Guid ProductVariantId { get; set; }

    /// <summary>
    /// Gets or sets the product name snapshot.
    /// </summary>
    public required string ProductName { get; set; }

    /// <summary>
    /// Gets or sets the SKU.
    /// </summary>
    public required string Sku { get; set; }

    /// <summary>
    /// Gets or sets the unit price.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the line total.
    /// </summary>
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Entity framework configuration for <see cref="OrderItem"/>.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.HasOne(i => i.Order)
               .WithMany(o => o.Items)
               .HasForeignKey(i => i.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Property(i => i.Sku).HasMaxLength(100).IsRequired();
        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
    }
}
