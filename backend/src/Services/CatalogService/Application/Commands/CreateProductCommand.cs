using MassTransit;
using MediatR;
using CatalogService.Api.Endpoints;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using SaaSCommon.Messaging.IntegrationEvents;

namespace CatalogService.Application.Commands;

/// <summary>
/// Command to create a new product.
/// </summary>
public record CreateProductCommand(
    Guid TenantId,
    string Name,
    string? Description,
    string Sku,
    decimal BasePrice,
    decimal? SalePrice,
    string Currency,
    Guid CategoryId
) : IRequest<ProductResponse>;

/// <summary>
/// Handler for <see cref="CreateProductCommand"/>.
/// </summary>
public class CreateProductCommandHandler(
    CatalogDbContext db,
    IPublishEndpoint publishEndpoint) : IRequestHandler<CreateProductCommand, ProductResponse>
{
    /// <inheritdoc />
    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
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
        await db.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new ProductCreatedIntegrationEvent
        {
            ProductId = product.Id,
            TenantId = product.TenantId,
            Name = product.Name,
            Sku = product.Sku,
            Price = product.BasePrice,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        return MapToResponse(product);
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
