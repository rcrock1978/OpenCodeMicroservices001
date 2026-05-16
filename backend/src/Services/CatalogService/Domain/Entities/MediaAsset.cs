using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Domain.Entities;

/// <summary>
/// Represents a media asset (image, video) for a product.
/// </summary>
public class MediaAsset
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
    /// Gets or sets the product identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Navigation property for the product.
    /// </summary>
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Gets or sets the asset URL.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the asset type (Image, Video, Document).
    /// </summary>
    public MediaAssetType Type { get; set; } = MediaAssetType.Image;

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Gets or sets when the asset was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Defines the possible media asset types.
/// </summary>
public enum MediaAssetType
{
    Image,
    Video,
    Document
}

/// <summary>
/// Entity framework configuration for <see cref="MediaAsset"/>.
/// </summary>
public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.TenantId, m.ProductId });
        builder.Property(m => m.Url).HasMaxLength(1000).IsRequired();
        builder.HasOne(m => m.Product)
               .WithMany()
               .HasForeignKey(m => m.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
