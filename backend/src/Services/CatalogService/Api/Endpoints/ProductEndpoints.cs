using MassTransit;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging.IntegrationEvents;

namespace CatalogService.Api.Endpoints;

/// <summary>
/// API endpoints for product management.
/// </summary>
public static class ProductEndpoints
{
    /// <summary>
    /// Maps product-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products").WithOpenApi();

        group.MapGet("/", async (CatalogDbContext db) =>
        {
            var products = await db.Products.AsNoTracking()
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .ToListAsync();
            return Results.Ok(products.Select(MapToResponse));
        });

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, CatalogDbContext db) =>
        {
            var products = await db.Products.AsNoTracking()
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => p.TenantId == tenantId && p.IsActive)
                .ToListAsync();
            return Results.Ok(products.Select(MapToResponse));
        });

        group.MapGet("/{id:guid}", async (Guid id, CatalogDbContext db) =>
            await db.Products.AsNoTracking()
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id) is Product product
                ? Results.Ok(MapToResponse(product))
                : Results.NotFound());

        group.MapPost("/", async (CreateProductRequest request, CatalogDbContext db, IPublishEndpoint publishEndpoint) =>
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Name = request.Name,
                Description = request.Description,
                Sku = request.Sku,
                BasePrice = request.BasePrice,
                SalePrice = request.SalePrice,
                Currency = request.Currency,
                CategoryId = request.CategoryId,
                IsActive = true
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            await publishEndpoint.Publish(new ProductCreatedIntegrationEvent
            {
                ProductId = product.Id,
                TenantId = product.TenantId,
                Name = product.Name,
                Sku = product.Sku,
                Price = product.BasePrice,
                CreatedAt = DateTimeOffset.UtcNow
            });

            return Results.Created($"/api/products/{product.Id}", MapToResponse(product));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, CatalogDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();
            product.Name = request.Name;
            product.Description = request.Description;
            product.BasePrice = request.BasePrice;
            product.SalePrice = request.SalePrice;
            product.IsActive = request.IsActive;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, CatalogDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();
            product.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }

    private static ProductResponse MapToResponse(Product product) =>
        new(
            product.Id,
            product.TenantId,
            product.Name,
            product.Description,
            product.Sku,
            product.BasePrice,
            product.SalePrice,
            product.Currency,
            product.CategoryId,
            product.Category is null ? null : new CategorySummaryResponse(
                product.Category.Id,
                product.Category.TenantId,
                product.Category.Name,
                product.Category.ParentCategoryId,
                product.Category.IsActive),
            product.IsActive,
            product.CreatedAt,
            product.Variants.Select(v => new ProductVariantResponse(
                v.Id,
                v.ProductId,
                v.Name,
                v.Sku,
                v.PriceOverride,
                v.Attributes)).ToList()
        );
}

/// <summary>
/// API endpoints for category management.
/// </summary>
public static class CategoryEndpoints
{
    /// <summary>
    /// Maps category-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories").WithOpenApi();

        group.MapGet("/", async (CatalogDbContext db) =>
        {
            var categories = await db.Categories.AsNoTracking().ToListAsync();
            return Results.Ok(categories.Select(c => new CategoryResponse(
                c.Id, c.TenantId, c.Name, c.ParentCategoryId, c.IsActive)));
        });

        group.MapPost("/", async (CreateCategoryRequest request, CatalogDbContext db) =>
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Name = request.Name,
                ParentCategoryId = request.ParentCategoryId
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return Results.Created($"/api/categories/{category.Id}", new CategoryResponse(
                category.Id, category.TenantId, category.Name, category.ParentCategoryId, category.IsActive));
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a product.
/// </summary>
public record CreateProductRequest(Guid TenantId, string Name, string? Description, string Sku, decimal BasePrice, decimal? SalePrice, string Currency, Guid CategoryId);

/// <summary>
/// Request model for updating a product.
/// </summary>
public record UpdateProductRequest(string Name, string? Description, decimal BasePrice, decimal? SalePrice, bool IsActive);

/// <summary>
/// Request model for creating a category.
/// </summary>
public record CreateCategoryRequest(Guid TenantId, string Name, Guid? ParentCategoryId);
