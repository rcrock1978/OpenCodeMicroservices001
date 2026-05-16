using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Sagas;

namespace OrderService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for MassTransit saga state persistence.
/// Stores state machine instances for the Order Service sagas.
/// </summary>
public class OrderSagaDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderSagaDbContext"/> class.
    /// </summary>
    public OrderSagaDbContext(DbContextOptions<OrderSagaDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the order placement saga states DbSet.
    /// </summary>
    public DbSet<OrderPlacementState> OrderPlacementStates => Set<OrderPlacementState>();

    /// <summary>
    /// Gets or sets the order cancellation saga states DbSet.
    /// </summary>
    public DbSet<OrderCancellationState> OrderCancellationStates => Set<OrderCancellationState>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderPlacementState>(entity =>
        {
            entity.HasKey(e => e.CorrelationId);
            entity.Property(e => e.CurrentState).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Version).IsConcurrencyToken();
            entity.HasIndex(e => e.OrderId).IsUnique();
            entity.HasIndex(e => e.TenantId);
        });

        modelBuilder.Entity<OrderCancellationState>(entity =>
        {
            entity.HasKey(e => e.CorrelationId);
            entity.Property(e => e.CurrentState).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Version).IsConcurrencyToken();
            entity.HasIndex(e => e.OrderId).IsUnique();
            entity.HasIndex(e => e.TenantId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
