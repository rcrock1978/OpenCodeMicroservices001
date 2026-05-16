using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Domain.Entities;

/// <summary>
/// Represents a product in the ecommerce catalog.
/// </summary>
public class Product
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
    /// Gets or sets the product name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the SKU.
    /// </summary>
    public required string Sku { get; set; }

    /// <summary>
    /// Gets or sets the base price.
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Gets or sets the sale price (null if not on sale).
    /// </summary>
    public decimal? SalePrice { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Navigation property for the category.
    /// </summary>
    public Category Category { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether the product is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets when the product was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for product variants.
    /// </summary>
    public ICollection<ProductVariant> Variants { get; set; } = [];
}

/// <summary>
/// Entity framework configuration for <see cref="Product"/>.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.TenantId, p.Sku }).IsUnique();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Sku).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Represents a product variant (e.g., size, color).
/// </summary>
public class ProductVariant
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Navigation property for the product.
    /// </summary>
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Gets or sets the variant name (e.g., "Navy / Medium").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the variant SKU.
    /// </summary>
    public required string Sku { get; set; }

    /// <summary>
    /// Gets or sets the variant-specific price override.
    /// </summary>
    public decimal? PriceOverride { get; set; }

    /// <summary>
    /// Gets or sets the variant attributes as JSON.
    /// </summary>
    public string? Attributes { get; set; }
}

/// <summary>
/// Entity framework configuration for <see cref="ProductVariant"/>.
/// </summary>
public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => v.Sku).IsUnique();
        builder.Property(v => v.Name).HasMaxLength(200).IsRequired();
        builder.Property(v => v.Sku).HasMaxLength(100).IsRequired();
        builder.HasOne(v => v.Product)
               .WithMany(p => p.Variants)
               .HasForeignKey(v => v.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Represents a product category.
/// </summary>
public class Category
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
    /// Gets or sets the category name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the parent category identifier.
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Navigation property for parent category.
    /// </summary>
    public Category? ParentCategory { get; set; }

    /// <summary>
    /// Gets or sets whether the category is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for products in this category.
    /// </summary>
    public ICollection<Product> Products { get; set; } = [];
}

/// <summary>
/// Entity framework configuration for <see cref="Category"/>.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.HasOne(c => c.ParentCategory)
               .WithMany()
               .HasForeignKey(c => c.ParentCategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
