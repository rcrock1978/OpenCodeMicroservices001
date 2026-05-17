using MediatR;
using CatalogService.Api.Endpoints;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Queries;

/// <summary>
/// Query to retrieve a product by its unique identifier.
/// </summary>
public record GetProductByIdQuery(Guid Id) : IRequest<ProductResponse?>;

/// <summary>
/// Handler for <see cref="GetProductByIdQuery"/>.
/// </summary>
public class GetProductByIdQueryHandler(CatalogDbContext db) : IRequestHandler<GetProductByIdQuery, ProductResponse?>
{
    /// <inheritdoc />
    public async Task<ProductResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking()
            .Include(p => p.Variants)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return product is null ? null : MapToResponse(product);
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
