using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Product"/> entity and related configurations.
/// </summary>
public class ProductTests
{
    #region Product Creation & Properties

    [Fact]
    public void Product_Created_WithRequiredProperties_ShouldSucceed()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test Product",
            Sku = "TEST-001",
            BasePrice = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.NotEqual(Guid.Empty, product.TenantId);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("TEST-001", product.Sku);
        Assert.Equal(99.99m, product.BasePrice);
        Assert.NotEqual(Guid.Empty, product.CategoryId);
    }

    [Fact]
    public void Product_DefaultValues_ShouldBeSet()
    {
        var product = new Product
        {
            Name = "Default Test",
            Sku = "DEFAULT-001"
        };

        Assert.True(product.IsActive);
        Assert.Equal("USD", product.Currency);
        Assert.True(product.CreatedAt <= DateTime.UtcNow);
        Assert.NotNull(product.Variants);
        Assert.Empty(product.Variants);
    }

    [Fact]
    public void Product_SalePrice_ShouldBeNullByDefault()
    {
        var product = new Product
        {
            Name = "No Sale",
            Sku = "NOSALE-001"
        };

        Assert.Null(product.SalePrice);
    }

    [Fact]
    public void Product_SalePrice_CanBeSet()
    {
        var product = new Product
        {
            Name = "On Sale",
            Sku = "SALE-001",
            SalePrice = 49.99m
        };

        Assert.Equal(49.99m, product.SalePrice);
    }

    [Fact]
    public void Product_Description_CanBeNull()
    {
        var product = new Product
        {
            Name = "No Description",
            Sku = "NODESC-001"
        };

        Assert.Null(product.Description);
    }

    [Fact]
    public void Product_Description_CanBeSet()
    {
        var product = new Product
        {
            Name = "With Description",
            Sku = "DESC-001",
            Description = "A detailed description"
        };

        Assert.Equal("A detailed description", product.Description);
    }

    #endregion

    #region Product Relationships

    [Fact]
    public void Product_Category_CanBeAssigned()
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test Category"
        };

        var product = new Product
        {
            Name = "Categorized Product",
            Sku = "CAT-001",
            CategoryId = category.Id,
            Category = category
        };

        Assert.NotNull(product.Category);
        Assert.Equal(category.Id, product.CategoryId);
        Assert.Equal("Test Category", product.Category.Name);
    }

    [Fact]
    public void Product_Variants_CanBeAdded()
    {
        var product = new Product
        {
            Name = "Variant Product",
            Sku = "VAR-001"
        };

        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Size L",
            Sku = "VAR-001-L"
        };

        product.Variants.Add(variant);

        Assert.Single(product.Variants);
        Assert.Contains(variant, product.Variants);
    }

    #endregion

    #region ProductConfiguration

    [Fact]
    public void ProductConfiguration_HasCorrectPrimaryKey()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Product));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties[0].Name);
    }

    [Fact]
    public void ProductConfiguration_HasUniqueIndexOnTenantIdAndSku()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Product));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantSkuIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "Sku"));

        Assert.NotNull(tenantSkuIndex);
        Assert.True(tenantSkuIndex.IsUnique);
    }

    [Fact]
    public void ProductConfiguration_Name_HasMaxLength200AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Product));

        Assert.NotNull(entityType);
        var nameProperty = entityType.FindProperty("Name");
        Assert.NotNull(nameProperty);
        Assert.Equal(200, nameProperty.GetMaxLength());
        Assert.False(nameProperty.IsNullable);
    }

    [Fact]
    public void ProductConfiguration_Sku_HasMaxLength100AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Product));

        Assert.NotNull(entityType);
        var skuProperty = entityType.FindProperty("Sku");
        Assert.NotNull(skuProperty);
        Assert.Equal(100, skuProperty.GetMaxLength());
        Assert.False(skuProperty.IsNullable);
    }

    [Fact]
    public void ProductConfiguration_Currency_HasMaxLength3AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Product));

        Assert.NotNull(entityType);
        var currencyProperty = entityType.FindProperty("Currency");
        Assert.NotNull(currencyProperty);
        Assert.Equal(3, currencyProperty.GetMaxLength());
        Assert.False(currencyProperty.IsNullable);
    }

    [Fact]
    public void ProductConfiguration_HasForeignKeyToCategory()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Product));

        Assert.NotNull(entityType);
        var foreignKey = entityType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Category));

        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    #endregion

    #region ProductVariant Tests

    [Fact]
    public void ProductVariant_Created_WithRequiredProperties_ShouldSucceed()
    {
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Name = "Large / Blue",
            Sku = "PROD-L-BLU"
        };

        Assert.NotEqual(Guid.Empty, variant.Id);
        Assert.NotEqual(Guid.Empty, variant.ProductId);
        Assert.Equal("Large / Blue", variant.Name);
        Assert.Equal("PROD-L-BLU", variant.Sku);
    }

    [Fact]
    public void ProductVariant_PriceOverride_CanBeNull()
    {
        var variant = new ProductVariant
        {
            Name = "Standard",
            Sku = "STD-001"
        };

        Assert.Null(variant.PriceOverride);
    }

    [Fact]
    public void ProductVariant_PriceOverride_CanBeSet()
    {
        var variant = new ProductVariant
        {
            Name = "Premium",
            Sku = "PRM-001",
            PriceOverride = 149.99m
        };

        Assert.Equal(149.99m, variant.PriceOverride);
    }

    [Fact]
    public void ProductVariantConfiguration_HasUniqueSkuIndex()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(ProductVariant));

        Assert.NotNull(entityType);
        var skuIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1 && i.Properties[0].Name == "Sku");

        Assert.NotNull(skuIndex);
        Assert.True(skuIndex.IsUnique);
    }

    [Fact]
    public void ProductVariantConfiguration_HasCascadeDeleteToProduct()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(ProductVariant));

        Assert.NotNull(entityType);
        var foreignKey = entityType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Product));

        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    #endregion

    #region Category Tests

    [Fact]
    public void Category_Created_WithRequiredProperties_ShouldSucceed()
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Electronics"
        };

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.NotEqual(Guid.Empty, category.TenantId);
        Assert.Equal("Electronics", category.Name);
    }

    [Fact]
    public void Category_DefaultValues_ShouldBeSet()
    {
        var category = new Category
        {
            Name = "Test Category"
        };

        Assert.True(category.IsActive);
        Assert.Null(category.ParentCategoryId);
        Assert.Null(category.ParentCategory);
        Assert.NotNull(category.Products);
        Assert.Empty(category.Products);
    }

    [Fact]
    public void Category_ParentCategory_CanBeAssigned()
    {
        var parent = new Category
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Parent"
        };

        var child = new Category
        {
            Id = Guid.NewGuid(),
            TenantId = parent.TenantId,
            Name = "Child",
            ParentCategoryId = parent.Id,
            ParentCategory = parent
        };

        Assert.NotNull(child.ParentCategory);
        Assert.Equal(parent.Id, child.ParentCategoryId);
        Assert.Equal("Parent", child.ParentCategory.Name);
    }

    [Fact]
    public void CategoryConfiguration_Name_HasMaxLength200AndIsRequired()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Category));

        Assert.NotNull(entityType);
        var nameProperty = entityType.FindProperty("Name");
        Assert.NotNull(nameProperty);
        Assert.Equal(200, nameProperty.GetMaxLength());
        Assert.False(nameProperty.IsNullable);
    }

    [Fact]
    public void CategoryConfiguration_ParentCategory_HasRestrictDelete()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Category));

        Assert.NotNull(entityType);
        var foreignKey = entityType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Category));

        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Product_WithCategoryAndVariants_FullGraph_CanBeConstructed()
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Apparel"
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = category.TenantId,
            Name = "T-Shirt",
            Sku = "TS-001",
            BasePrice = 29.99m,
            SalePrice = 19.99m,
            CategoryId = category.Id,
            Category = category
        };

        var variant1 = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Small / Red",
            Sku = "TS-001-S-RED",
            PriceOverride = 24.99m
        };

        var variant2 = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Large / Blue",
            Sku = "TS-001-L-BLU"
        };

        product.Variants.Add(variant1);
        product.Variants.Add(variant2);
        category.Products.Add(product);

        Assert.Equal(2, product.Variants.Count);
        Assert.Single(category.Products);
        Assert.Equal(category, product.Category);
        Assert.Equal(product.Id, variant1.ProductId);
    }

    [Fact]
    public void ProductConfiguration_SavedToDatabase_RetainsValues()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        using (var context = new TestCatalogDbContext(options))
        {
            context.Products.Add(new Product
            {
                Id = productId,
                TenantId = tenantId,
                Name = "Persisted Product",
                Sku = "PERS-001",
                BasePrice = 100.00m,
                Description = "A test product",
                IsActive = true
            });
            context.SaveChanges();
        }

        using (var context = new TestCatalogDbContext(options))
        {
            var product = context.Products.Find(productId);

            Assert.NotNull(product);
            Assert.Equal("Persisted Product", product.Name);
            Assert.Equal("PERS-001", product.Sku);
            Assert.Equal(100.00m, product.BasePrice);
            Assert.Equal("A test product", product.Description);
            Assert.True(product.IsActive);
            Assert.Equal(tenantId, product.TenantId);
        }
    }

    [Fact]
    public void ProductConfiguration_UniqueTenantSku_IndexExists()
    {
        var options = new DbContextOptionsBuilder<TestCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestCatalogDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Product));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();
        var tenantSkuIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "Sku"));

        Assert.NotNull(tenantSkuIndex);
        Assert.True(tenantSkuIndex.IsUnique);
    }

    #endregion
}

/// <summary>
/// In-memory test DbContext for verifying EF Core configurations.
/// </summary>
public class TestCatalogDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<MediaAsset> MediaAssets { get; set; } = null!;

    public TestCatalogDbContext(DbContextOptions<TestCatalogDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductVariantConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new MediaAssetConfiguration());
    }
}
