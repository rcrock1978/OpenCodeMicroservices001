using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the Catalog Service.
/// </summary>
public class CatalogDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CatalogDbContext"/> class.
    /// </summary>
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the products DbSet.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Gets or sets the product variants DbSet.
    /// </summary>
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    /// <summary>
    /// Gets or sets the categories DbSet.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductVariantConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
