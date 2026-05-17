using MediatR;
using CatalogService.Application.Commands;
using CatalogService.Application.Queries;
using CatalogService.Domain.Entities;

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

        group.MapGet("/", async (IMediator mediator) =>
        {
            var products = await mediator.Send(new GetProductsQuery());
            return Results.Ok(products);
        });

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, IMediator mediator) =>
        {
            var products = await mediator.Send(new GetProductsByTenantQuery(tenantId));
            return Results.Ok(products);
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var product = await mediator.Send(new GetProductByIdQuery(id));
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        group.MapPost("/", async (CreateProductRequest request, IMediator mediator) =>
        {
            var command = new CreateProductCommand(
                request.TenantId,
                request.Name,
                request.Description,
                request.Sku,
                request.BasePrice,
                request.SalePrice,
                request.Currency,
                request.CategoryId);

            var product = await mediator.Send(command);
            return Results.Created($"/api/products/{product.Id}", product);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IMediator mediator) =>
        {
            var command = new UpdateProductCommand(
                id,
                request.Name,
                request.Description,
                request.BasePrice,
                request.SalePrice,
                request.IsActive);

            var success = await mediator.Send(command);
            return success ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var success = await mediator.Send(new DeleteProductCommand(id));
            return success ? Results.NoContent() : Results.NotFound();
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

        group.MapGet("/", async (IMediator mediator) =>
        {
            var categories = await mediator.Send(new GetCategoriesQuery());
            return Results.Ok(categories);
        });

        group.MapPost("/", async (CreateCategoryRequest request, IMediator mediator) =>
        {
            var command = new CreateCategoryCommand(
                request.TenantId,
                request.Name,
                request.ParentCategoryId);

            var category = await mediator.Send(command);
            return Results.Created($"/api/categories/{category.Id}", category);
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
