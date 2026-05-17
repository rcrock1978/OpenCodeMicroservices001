namespace CatalogService.Api.Endpoints;

/// <summary>
/// Response model for a product.
/// </summary>
public record ProductResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    string Sku,
    decimal BasePrice,
    decimal? SalePrice,
    string Currency,
    Guid CategoryId,
    CategorySummaryResponse? Category,
    bool IsActive,
    DateTime CreatedAt,
    List<ProductVariantResponse> Variants
);

/// <summary>
/// Response model for a product variant.
/// </summary>
public record ProductVariantResponse(
    Guid Id,
    Guid ProductId,
    string Name,
    string Sku,
    decimal? PriceOverride,
    string? Attributes
);

/// <summary>
/// Summary response model for a category (without nested collections to avoid cycles).
/// </summary>
public record CategorySummaryResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    Guid? ParentCategoryId,
    bool IsActive
);

/// <summary>
/// Response model for a category.
/// </summary>
public record CategoryResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    Guid? ParentCategoryId,
    bool IsActive
);
