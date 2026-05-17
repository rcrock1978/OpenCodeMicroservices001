using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests.Infrastructure.Persistence;

/// <summary>
/// Unit tests for the <see cref="CatalogDbContext"/> database context.
/// </summary>
public class CatalogDbContextTests
{
    private static DbContextOptions<CatalogDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    #region Constructor & DbSets

    [Fact]
    public void CatalogDbContext_Constructor_WithValidOptions_ShouldCreateInstance()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);

        Assert.NotNull(context);
    }

    [Fact]
    public void CatalogDbContext_ProductsDbSet_ShouldNotBeNull()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);

        Assert.NotNull(context.Products);
    }

    [Fact]
    public void CatalogDbContext_ProductVariantsDbSet_ShouldNotBeNull()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);

        Assert.NotNull(context.ProductVariants);
    }

    [Fact]
    public void CatalogDbContext_CategoriesDbSet_ShouldNotBeNull()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);

        Assert.NotNull(context.Categories);
    }

    [Fact]
    public void CatalogDbContext_MediaAssetsDbSet_ShouldNotBeNull()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);

        Assert.NotNull(context.MediaAssets);
    }

    #endregion

    #region Model Configuration

    [Fact]
    public void CatalogDbContext_ModelCreating_AppliesProductConfiguration()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Product));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    [Fact]
    public void CatalogDbContext_ModelCreating_AppliesProductVariantConfiguration()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(ProductVariant));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    [Fact]
    public void CatalogDbContext_ModelCreating_AppliesCategoryConfiguration()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Category));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    [Fact]
    public void CatalogDbContext_ModelCreating_AppliesMediaAssetConfiguration()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(MediaAsset));

        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindPrimaryKey());
    }

    #endregion

    #region Product CRUD Operations

    [Fact]
    public void CatalogDbContext_Product_Add_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            context.Products.Add(new Product
            {
                Id = productId,
                TenantId = tenantId,
                Name = "Test Product",
                Sku = "TEST-001",
                BasePrice = 99.99m,
                CategoryId = Guid.NewGuid()
            });
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var product = context.Products.Find(productId);
            Assert.NotNull(product);
            Assert.Equal("Test Product", product.Name);
            Assert.Equal("TEST-001", product.Sku);
        }
    }

    [Fact]
    public void CatalogDbContext_Product_Update_ShouldPersistChanges()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var productId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            context.Products.Add(new Product
            {
                Id = productId,
                TenantId = Guid.NewGuid(),
                Name = "Original Name",
                Sku = "ORIG-001",
                BasePrice = 50.00m,
                CategoryId = Guid.NewGuid()
            });
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var product = context.Products.Find(productId);
            Assert.NotNull(product);
            product.Name = "Updated Name";
            product.BasePrice = 75.00m;
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var product = context.Products.Find(productId);
            Assert.NotNull(product);
            Assert.Equal("Updated Name", product.Name);
            Assert.Equal(75.00m, product.BasePrice);
        }
    }

    [Fact]
    public void CatalogDbContext_Product_Delete_ShouldRemove()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var productId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            context.Products.Add(new Product
            {
                Id = productId,
                TenantId = Guid.NewGuid(),
                Name = "To Delete",
                Sku = "DEL-001",
                BasePrice = 10.00m,
                CategoryId = Guid.NewGuid()
            });
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var product = context.Products.Find(productId);
            Assert.NotNull(product);
            context.Products.Remove(product);
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var product = context.Products.Find(productId);
            Assert.Null(product);
        }
    }

    [Fact]
    public void CatalogDbContext_Product_QueryByTenantId_ShouldFilter()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            context.Products.AddRange(
                new Product
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantA,
                    Name = "Tenant A Product",
                    Sku = "TENA-001",
                    BasePrice = 10.00m,
                    CategoryId = Guid.NewGuid()
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantB,
                    Name = "Tenant B Product",
                    Sku = "TENB-001",
                    BasePrice = 20.00m,
                    CategoryId = Guid.NewGuid()
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantA,
                    Name = "Tenant A Product 2",
                    Sku = "TENA-002",
                    BasePrice = 30.00m,
                    CategoryId = Guid.NewGuid()
                });
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var tenantAProducts = context.Products.Where(p => p.TenantId == tenantA).ToList();
            var tenantBProducts = context.Products.Where(p => p.TenantId == tenantB).ToList();

            Assert.Equal(2, tenantAProducts.Count);
            Assert.Single(tenantBProducts);
            Assert.All(tenantAProducts, p => Assert.Equal(tenantA, p.TenantId));
            Assert.All(tenantBProducts, p => Assert.Equal(tenantB, p.TenantId));
        }
    }

    #endregion

    #region Category CRUD Operations

    [Fact]
    public void CatalogDbContext_Category_Add_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var categoryId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            context.Categories.Add(new Category
            {
                Id = categoryId,
                TenantId = Guid.NewGuid(),
                Name = "Electronics"
            });
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var category = context.Categories.Find(categoryId);
            Assert.NotNull(category);
            Assert.Equal("Electronics", category.Name);
        }
    }

    [Fact]
    public void CatalogDbContext_Category_WithParentCategory_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            context.Categories.Add(new Category
            {
                Id = parentId,
                TenantId = Guid.NewGuid(),
                Name = "Parent Category"
            });
            context.Categories.Add(new Category
            {
                Id = childId,
                TenantId = Guid.NewGuid(),
                Name = "Child Category",
                ParentCategoryId = parentId
            });
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var child = context.Categories.Find(childId);
            Assert.NotNull(child);
            Assert.Equal(parentId, child.ParentCategoryId);
        }
    }

    #endregion

    #region ProductVariant CRUD Operations

    [Fact]
    public void CatalogDbContext_ProductVariant_Add_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var variantId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            context.ProductVariants.Add(new ProductVariant
            {
                Id = variantId,
                ProductId = Guid.NewGuid(),
                Name = "Size L",
                Sku = "VAR-001-L"
            });
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var variant = context.ProductVariants.Find(variantId);
            Assert.NotNull(variant);
            Assert.Equal("Size L", variant.Name);
            Assert.Equal("VAR-001-L", variant.Sku);
        }
    }

    #endregion

    #region MediaAsset CRUD Operations

    [Fact]
    public void CatalogDbContext_MediaAsset_Add_ShouldPersist()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var assetId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            context.MediaAssets.Add(new MediaAsset
            {
                Id = assetId,
                TenantId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Url = "https://example.com/image.jpg",
                Type = MediaAssetType.Image,
                SortOrder = 1
            });
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var asset = context.MediaAssets.Find(assetId);
            Assert.NotNull(asset);
            Assert.Equal("https://example.com/image.jpg", asset.Url);
            Assert.Equal(MediaAssetType.Image, asset.Type);
            Assert.Equal(1, asset.SortOrder);
        }
    }

    #endregion

    #region Integration & Relationships

    [Fact]
    public void CatalogDbContext_ProductWithVariants_CanBeAddedTogether()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            var product = new Product
            {
                Id = productId,
                TenantId = tenantId,
                Name = "Multi-Variant Product",
                Sku = "MVP-001",
                BasePrice = 49.99m,
                CategoryId = Guid.NewGuid()
            };

            var variant = new ProductVariant
            {
                Id = variantId,
                ProductId = productId,
                Name = "Navy / Medium",
                Sku = "MVP-001-NVY-M"
            };

            context.Products.Add(product);
            context.ProductVariants.Add(variant);
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var product = context.Products.Find(productId);
            var variant = context.ProductVariants.Find(variantId);

            Assert.NotNull(product);
            Assert.NotNull(variant);
            Assert.Equal(productId, variant.ProductId);
        }
    }

    [Fact]
    public void CatalogDbContext_ProductWithCategory_CanBeAddedTogether()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            var category = new Category
            {
                Id = categoryId,
                TenantId = tenantId,
                Name = "Apparel"
            };

            var product = new Product
            {
                Id = productId,
                TenantId = tenantId,
                Name = "T-Shirt",
                Sku = "TS-001",
                BasePrice = 29.99m,
                CategoryId = categoryId
            };

            context.Categories.Add(category);
            context.Products.Add(product);
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var product = context.Products.Find(productId);
            var category = context.Categories.Find(categoryId);

            Assert.NotNull(product);
            Assert.NotNull(category);
            Assert.Equal(categoryId, product.CategoryId);
        }
    }

    [Fact]
    public void CatalogDbContext_ComplexGraph_CanBeAddedAndRetrieved()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            var category = new Category
            {
                Id = categoryId,
                TenantId = tenantId,
                Name = "Complex Category"
            };

            var product = new Product
            {
                Id = productId,
                TenantId = tenantId,
                Name = "Complex Product",
                Sku = "COMPLEX-001",
                BasePrice = 99.99m,
                CategoryId = categoryId
            };

            var variant = new ProductVariant
            {
                Id = variantId,
                ProductId = productId,
                Name = "Complex Variant",
                Sku = "COMPLEX-001-V1"
            };

            var asset = new MediaAsset
            {
                Id = assetId,
                TenantId = tenantId,
                ProductId = productId,
                Url = "https://example.com/complex.jpg",
                SortOrder = 0
            };

            context.Categories.Add(category);
            context.Products.Add(product);
            context.ProductVariants.Add(variant);
            context.MediaAssets.Add(asset);
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            Assert.NotNull(context.Categories.Find(categoryId));
            Assert.NotNull(context.Products.Find(productId));
            Assert.NotNull(context.ProductVariants.Find(variantId));
            Assert.NotNull(context.MediaAssets.Find(assetId));

            var product = context.Products.Find(productId);
            Assert.Equal(categoryId, product!.CategoryId);
        }
    }

    [Fact]
    public void CatalogDbContext_MultipleContexts_Isolated()
    {
        var optionsA = CreateInMemoryOptions("DB-A");
        var optionsB = CreateInMemoryOptions("DB-B");

        using (var contextA = new CatalogDbContext(optionsA))
        {
            contextA.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Name = "DB A Product",
                Sku = "DBA-001",
                BasePrice = 10.00m,
                CategoryId = Guid.NewGuid()
            });
            contextA.SaveChanges();
        }

        using (var contextB = new CatalogDbContext(optionsB))
        {
            contextB.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Name = "DB B Product",
                Sku = "DBB-001",
                BasePrice = 20.00m,
                CategoryId = Guid.NewGuid()
            });
            contextB.SaveChanges();
        }

        using (var contextA = new CatalogDbContext(optionsA))
        {
            using (var contextB = new CatalogDbContext(optionsB))
            {
                Assert.Single(contextA.Products);
                Assert.Single(contextB.Products);

                var productA = contextA.Products.First();
                var productB = contextB.Products.First();

                Assert.Equal("DB A Product", productA.Name);
                Assert.Equal("DB B Product", productB.Name);
            }
        }
    }

    #endregion

    #region Change Tracking

    [Fact]
    public void CatalogDbContext_SaveChanges_ReturnsAffectedCount()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);
        context.Products.Add(new Product
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Tracked Product",
            Sku = "TRACK-001",
            BasePrice = 10.00m,
            CategoryId = Guid.NewGuid()
        });

        var affected = context.SaveChanges();

        Assert.Equal(1, affected);
    }

    [Fact]
    public void CatalogDbContext_ChangeTracker_TracksAddedEntity()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());

        using var context = new CatalogDbContext(options);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Tracked",
            Sku = "TRK-001",
            BasePrice = 10.00m,
            CategoryId = Guid.NewGuid()
        };

        context.Products.Add(product);

        var entry = context.Entry(product);
        Assert.Equal(EntityState.Added, entry.State);
    }

    [Fact]
    public void CatalogDbContext_ChangeTracker_TracksModifiedEntity()
    {
        var options = CreateInMemoryOptions(Guid.NewGuid().ToString());
        var productId = Guid.NewGuid();

        using (var context = new CatalogDbContext(options))
        {
            context.Products.Add(new Product
            {
                Id = productId,
                TenantId = Guid.NewGuid(),
                Name = "Original",
                Sku = "MOD-001",
                BasePrice = 10.00m,
                CategoryId = Guid.NewGuid()
            });
            context.SaveChanges();
        }

        using (var context = new CatalogDbContext(options))
        {
            var product = context.Products.Find(productId);
            Assert.NotNull(product);
            product.Name = "Modified";

            var entry = context.Entry(product);
            Assert.Equal(EntityState.Modified, entry.State);
        }
    }

    #endregion
}
