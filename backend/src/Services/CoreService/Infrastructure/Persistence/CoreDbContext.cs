using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the Core Service.
/// </summary>
public class CoreDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreDbContext"/> class.
    /// </summary>
    public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the projects DbSet.
    /// </summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
