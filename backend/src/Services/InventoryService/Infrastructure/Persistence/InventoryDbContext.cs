using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the Inventory Service.
/// </summary>
public class InventoryDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryDbContext"/> class.
    /// </summary>
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the stock items DbSet.
    /// </summary>
    public DbSet<StockItem> StockItems => Set<StockItem>();

    /// <summary>
    /// Gets or sets the stock reservations DbSet.
    /// </summary>
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    /// <summary>
    /// Gets or sets the stock movements DbSet.
    /// </summary>
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StockItemConfiguration());
        modelBuilder.ApplyConfiguration(new StockReservationConfiguration());
        modelBuilder.ApplyConfiguration(new StockMovementConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
