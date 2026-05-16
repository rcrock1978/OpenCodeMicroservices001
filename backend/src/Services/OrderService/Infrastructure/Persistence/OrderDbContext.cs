using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the Order Service.
/// </summary>
public class OrderDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderDbContext"/> class.
    /// </summary>
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the orders DbSet.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Gets or sets the order items DbSet.
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Gets or sets the order status histories DbSet.
    /// </summary>
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderStatusHistoryConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
