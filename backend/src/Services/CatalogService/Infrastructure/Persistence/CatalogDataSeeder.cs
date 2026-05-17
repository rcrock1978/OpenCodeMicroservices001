using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Persistence;

/// <summary>
/// Seeds the CatalogService database with initial demo data.
/// </summary>
public static class CatalogDataSeeder
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly string[] MainCategoryNames =
    [
        "Clothing", "Electronics", "Home & Garden", "Sports", "Books",
        "Toys", "Health & Beauty", "Automotive", "Food & Beverage", "Pet Supplies"
    ];

    private static readonly string[][] SubCategoryNames =
    [
        ["Men's Wear", "Women's Wear", "Kids' Wear", "Shoes", "Accessories"],
        ["Smartphones", "Laptops", "Cameras", "Audio", "Wearables"],
        ["Furniture", "Decor", "Kitchen", "Lighting", "Bedding"],
        ["Fitness", "Outdoor", "Team Sports", "Cycling", "Water Sports"],
        ["Fiction", "Non-Fiction", "Educational", "Comics", "Magazines"],
        ["Action Figures", "Board Games", "Puzzles", "Dolls", "Educational Toys"],
        ["Skincare", "Haircare", "Makeup", "Vitamins", "Personal Care"],
        ["Car Parts", "Tools", "Accessories", "Tires", "Electronics"],
        ["Snacks", "Beverages", "Organic", "Frozen", "Spices"],
        ["Dog Supplies", "Cat Supplies", "Fish", "Birds", "Small Pets"]
    ];

    private static readonly string[] ProductAdjectives =
    [
        "Premium", "Classic", "Modern", "Eco-Friendly", "Compact", "Professional",
        "Portable", "Wireless", "Smart", "Durable", "Lightweight", "Ergonomic",
        "Stylish", "Advanced", "Essential", "Deluxe", "Vintage", "Minimalist",
        "High-Performance", "Budget", "Luxury", "Rugged", "Sleek", "Versatile"
    ];

    private static readonly string[] ProductNouns =
    [
        "Shirt", "Jacket", "Sneakers", "Phone", "Laptop", "Headphones", "Watch",
        "Sofa", "Lamp", "Table", "Yoga Mat", "Bicycle", "Novel", "Cookbook",
        "Robot", "Board Game", "Serum", "Shampoo", "Toolkit", "Tire", "Coffee",
        "Tea", "Dog Bed", "Cat Tree", "Backpack", "Camera", "Speaker", "Rug",
        "Mirror", "Blender", "Dumbbell", "Tent", "Kayak", "Journal", " Puzzle",
        "Action Figure", "Lipstick", "Moisturizer", "Dash Cam", "Car Cover",
        "Granola", "Juice", "Fish Tank", "Bird Cage", "Hamster Wheel"
    ];

    private static readonly string[] Colors = ["Red", "Blue", "Green", "Black", "White", "Navy", "Gray", "Pink", "Yellow", "Purple"];
    private static readonly string[] Sizes = ["XS", "S", "M", "L", "XL", "XXL"];

    /// <summary>
    /// Seeds the database with 1000 products, categories, variants, and media assets.
    /// </summary>
    public static async Task SeedAsync(CatalogDbContext dbContext, ILogger logger)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Products.AnyAsync())
        {
            logger.LogInformation("Database already contains products. Skipping seed.");
            return;
        }

        var rng = new Random(42); // deterministic seed

        // ---- Categories ----
        var categories = new List<Category>();
        var categoryMap = new Dictionary<string, Category>();

        for (int i = 0; i < MainCategoryNames.Length; i++)
        {
            var mainCat = new Category
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                Name = MainCategoryNames[i],
                IsActive = true
            };
            categories.Add(mainCat);
            categoryMap[MainCategoryNames[i]] = mainCat;

            foreach (var subName in SubCategoryNames[i])
            {
                var subCat = new Category
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    Name = subName,
                    ParentCategoryId = mainCat.Id,
                    IsActive = true
                };
                categories.Add(subCat);
                categoryMap[$"{MainCategoryNames[i]}:{subName}"] = subCat;
            }
        }

        await dbContext.Categories.AddRangeAsync(categories);
        await dbContext.SaveChangesAsync();

        // Flatten leaf categories (subcategories) for product assignment
        var leafCategories = categories.Where(c => c.ParentCategoryId.HasValue).ToList();

        // ---- Products, Variants, Media ----
        const int productCount = 1000;
        var products = new List<Product>();
        var variants = new List<ProductVariant>();
        var mediaAssets = new List<MediaAsset>();

        for (int i = 0; i < productCount; i++)
        {
            var category = leafCategories[rng.Next(leafCategories.Count)];
            var adj = ProductAdjectives[rng.Next(ProductAdjectives.Length)];
            var noun = ProductNouns[rng.Next(ProductNouns.Length)];
            var name = $"{adj} {noun} {i + 1:0000}";
            var sku = $"SKU-{i + 1:000000}";
            var basePrice = Math.Round((decimal)(rng.NextDouble() * 490 + 10), 2); // 10.00 - 500.00
            var hasSale = rng.NextDouble() < 0.2;
            var salePrice = hasSale ? Math.Round(basePrice * (decimal)(rng.NextDouble() * 0.3 + 0.5), 2) : (decimal?)null; // 50-80% of base

            var product = new Product
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                Name = name,
                Description = $"A {adj.ToLowerInvariant()} {noun.ToLowerInvariant()} perfect for everyday use. Item #{i + 1}.",
                Sku = sku,
                BasePrice = basePrice,
                SalePrice = salePrice,
                Currency = "USD",
                CategoryId = category.Id,
                IsActive = rng.NextDouble() < 0.95,
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(0, 365))
            };
            products.Add(product);

            // Media assets (1-2 per product)
            var mediaCount = rng.Next(1, 3);
            for (int m = 0; m < mediaCount; m++)
            {
                mediaAssets.Add(new MediaAsset
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    ProductId = product.Id,
                    Url = $"https://example.com/media/{product.Id}/{m + 1}.jpg",
                    Type = MediaAssetType.Image,
                    SortOrder = m,
                    CreatedAt = product.CreatedAt
                });
            }

            // Variants (0-2 per product)
            var variantCount = rng.Next(0, 3);
            for (int v = 0; v < variantCount; v++)
            {
                var color = Colors[rng.Next(Colors.Length)];
                var size = Sizes[rng.Next(Sizes.Length)];
                var varName = $"{color} / {size}";
                var varSku = $"{sku}-V{v + 1:00}";
                var priceOverride = rng.NextDouble() < 0.3 ? Math.Round(basePrice + (decimal)(rng.NextDouble() * 20 - 10), 2) : (decimal?)null;

                variants.Add(new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Name = varName,
                    Sku = varSku,
                    PriceOverride = priceOverride,
                    Attributes = $"{{\"color\":\"{color}\",\"size\":\"{size}\"}}"
                });
            }
        }

        await dbContext.Products.AddRangeAsync(products);
        await dbContext.MediaAssets.AddRangeAsync(mediaAssets);
        await dbContext.ProductVariants.AddRangeAsync(variants);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Seeded CatalogService with {ProductCount} products, {CategoryCount} categories, {VariantCount} variants, and {MediaCount} media assets.",
            products.Count, categories.Count, variants.Count, mediaAssets.Count);
    }
}
