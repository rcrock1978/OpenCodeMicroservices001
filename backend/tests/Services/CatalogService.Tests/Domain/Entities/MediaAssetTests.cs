using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="MediaAsset"/> entity and related configurations.
/// </summary>
public class MediaAssetTests
{
    #region MediaAsset Creation & Properties

    [Fact]
    public void MediaAsset_Created_WithRequiredProperties_ShouldSucceed()
    {
        var asset = new MediaAsset
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Url = "https://example.com/image.jpg"
        };

        Assert.NotEqual(Guid.Empty, asset.Id);
        Assert.NotEqual(Guid.Empty, asset.TenantId);
        Assert.NotEqual(Guid.Empty, asset.ProductId);
        Assert.Equal("https://example.com/image.jpg", asset.Url);
    }

    [Fact]
    public void MediaAsset_DefaultValues_ShouldBeSet()
    {
        var asset = new MediaAsset
        {
            Url = "https://example.com/test.jpg"
        };

        Assert.Equal(MediaAssetType.Image, asset.Type);
        Assert.Equal(0, asset.SortOrder);
        Assert.True(asset.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MediaAsset_Type_CanBeSetToVideo()
    {
        var asset = new MediaAsset
        {
            Url = "https://example.com/video.mp4",
            Type = MediaAssetType.Video
        };

        Assert.Equal(MediaAssetType.Video, asset.Type);
    }

    [Fact]
    public void MediaAsset_Type_CanBeSetToDocument()
    {
        var asset = new MediaAsset
        {
            Url = "https://example.com/manual.pdf",
            Type = MediaAssetType.Document
        };

        Assert.Equal(MediaAssetType.Document, asset.Type);
    }

    [Fact]
    public void MediaAsset_SortOrder_CanBeSet()
    {
        var asset = new MediaAsset
        {
            Url = "https://example.com/image.jpg",
            SortOrder = 5
        };

        Assert.Equal(5, asset.SortOrder);
    }

    [Fact]
    public void MediaAsset_SortOrder_DefaultsToZero()
    {
        var asset = new MediaAsset
        {
            Url = "https://example.com/image.jpg"
        };

        Assert.Equal(0, asset.SortOrder);
    }

    [Fact]
    public void MediaAsset_Url_CanBeLongUrl()
    {
        var longUrl = "https://example.com/very/long/path/to/the/asset/file/name/that/is/extended/" + new string('x', 200);
        var asset = new MediaAsset
        {
            Url = longUrl
        };

        Assert.Equal(longUrl, asset.Url);
    }

    #endregion

    #region MediaAsset Relationships

    [Fact]
    public void MediaAsset_Product_CanBeAssigned()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test Product",
            Sku = "MEDIA-001"
        };

        var asset = new MediaAsset
        {
            Url = "https://example.com/product.jpg",
            ProductId = product.Id,
            Product = product
        };

        Assert.NotNull(asset.Product);
        Assert.Equal(product.Id, asset.ProductId);
        Assert.Equal("Test Product", asset.Product.Name);
    }

    #endregion

    #region MediaAssetConfiguration

    [Fact]
    public void MediaAssetConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(MediaAsset));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void MediaAssetConfiguration_HasIndexOnTenantIdAndProductId()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(MediaAsset));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantProductIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "ProductId"));

        Assert.NotNull(tenantProductIndex);
    }

    [Fact]
    public void MediaAssetConfiguration_Url_HasMaxLength1000AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(MediaAsset));

        Assert.NotNull(entityType);
        var urlProperty = entityType.FindProperty("Url");
        Assert.NotNull(urlProperty);
        Assert.Equal(1000, urlProperty.GetMaxLength());
        Assert.False(urlProperty.IsNullable);
    }

    [Fact]
    public void MediaAssetConfiguration_HasForeignKeyToProduct()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(MediaAsset));

        Assert.NotNull(entityType);
        var foreignKey = entityType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Product));

        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    #endregion

    #region MediaAssetType Enum Tests

    [Theory]
    [InlineData(MediaAssetType.Image)]
    [InlineData(MediaAssetType.Video)]
    [InlineData(MediaAssetType.Document)]
    public void MediaAssetType_ValidValues_ShouldExist(MediaAssetType type)
    {
        // Verify each enum value can be assigned
        var asset = new MediaAsset
        {
            Url = "https://example.com/asset",
            Type = type
        };

        Assert.Equal(type, asset.Type);
    }

    [Fact]
    public void MediaAssetType_Image_IsDefault()
    {
        var asset = new MediaAsset
        {
            Url = "https://example.com/asset"
        };

        Assert.Equal(MediaAssetType.Image, asset.Type);
    }

    [Fact]
    public void MediaAssetType_HasExactlyThreeValues()
    {
        var values = Enum.GetValues<MediaAssetType>();

        Assert.Equal(3, values.Length);
        Assert.Contains(MediaAssetType.Image, values);
        Assert.Contains(MediaAssetType.Video, values);
        Assert.Contains(MediaAssetType.Document, values);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MediaAsset_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var assetId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        using (var context = new TestCatalogDbContext(options))
        {
            context.MediaAssets.Add(new MediaAsset
            {
                Id = assetId,
                TenantId = tenantId,
                ProductId = productId,
                Url = "https://example.com/saved.jpg",
                Type = MediaAssetType.Video,
                SortOrder = 3
            });
            context.SaveChanges();
        }

        using (var context = new TestCatalogDbContext(options))
        {
            var asset = context.MediaAssets.Find(assetId);

            Assert.NotNull(asset);
            Assert.Equal("https://example.com/saved.jpg", asset.Url);
            Assert.Equal(MediaAssetType.Video, asset.Type);
            Assert.Equal(3, asset.SortOrder);
            Assert.Equal(tenantId, asset.TenantId);
            Assert.Equal(productId, asset.ProductId);
        }
    }

    [Fact]
    public void MediaAsset_MultipleAssetsForProduct_CanBeOrdered()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        using (var context = new TestCatalogDbContext(options))
        {
            context.MediaAssets.AddRange(
                new MediaAsset
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ProductId = productId,
                    Url = "https://example.com/third.jpg",
                    SortOrder = 3
                },
                new MediaAsset
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ProductId = productId,
                    Url = "https://example.com/first.jpg",
                    SortOrder = 1
                },
                new MediaAsset
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ProductId = productId,
                    Url = "https://example.com/second.jpg",
                    SortOrder = 2
                });
            context.SaveChanges();
        }

        using (var context = new TestCatalogDbContext(options))
        {
            var assets = context.MediaAssets
                .Where(a => a.ProductId == productId)
                .OrderBy(a => a.SortOrder)
                .ToList();

            Assert.Equal(3, assets.Count);
            Assert.Equal("https://example.com/first.jpg", assets[0].Url);
            Assert.Equal("https://example.com/second.jpg", assets[1].Url);
            Assert.Equal("https://example.com/third.jpg", assets[2].Url);
        }
    }

    [Fact]
    public void MediaAsset_CascadeDelete_ConfigurationIsSet()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(MediaAsset));

        Assert.NotNull(entityType);
        var foreignKey = entityType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Product));

        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    #endregion
}
