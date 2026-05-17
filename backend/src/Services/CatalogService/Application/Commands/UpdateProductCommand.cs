using MediatR;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Commands;

/// <summary>
/// Command to update an existing product.
/// </summary>
public record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal BasePrice,
    decimal? SalePrice,
    bool IsActive
) : IRequest<bool>;

/// <summary>
/// Handler for <see cref="UpdateProductCommand"/>.
/// </summary>
public class UpdateProductCommandHandler(CatalogDbContext db) : IRequestHandler<UpdateProductCommand, bool>
{
    /// <inheritdoc />
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([request.Id], cancellationToken);
        if (product is null) return false;

        product.Name = request.Name;
        product.Description = request.Description;
        product.BasePrice = request.BasePrice;
        product.SalePrice = request.SalePrice;
        product.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
