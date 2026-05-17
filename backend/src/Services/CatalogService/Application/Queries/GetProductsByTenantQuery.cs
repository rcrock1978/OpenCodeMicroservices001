using MediatR;
using CatalogService.Api.Endpoints;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Queries;

/// <summary>
/// Query to retrieve active products for a specific tenant.
/// </summary>
public record GetProductsByTenantQuery(Guid TenantId) : IRequest<List<ProductResponse>>;

/// <summary>
/// Handler for <see cref="GetProductsByTenantQuery"/>.
/// </summary>
public class GetProductsByTenantQueryHandler(CatalogDbContext db) : IRequestHandler<GetProductsByTenantQuery, List<ProductResponse>>
{
    /// <inheritdoc />
    public async Task<List<ProductResponse>> Handle(GetProductsByTenantQuery request, CancellationToken cancellationToken)
    {
        var products = await db.Products.AsNoTracking()
            .Include(p => p.Variants)
            .Include(p => p.Category)
            .Where(p => p.TenantId == request.TenantId && p.IsActive)
            .ToListAsync(cancellationToken);

        return products.Select(MapToResponse).ToList();
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
