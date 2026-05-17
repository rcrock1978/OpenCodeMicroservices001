using MediatR;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Commands;

/// <summary>
/// Command to soft-delete a product.
/// </summary>
public record DeleteProductCommand(Guid Id) : IRequest<bool>;

/// <summary>
/// Handler for <see cref="DeleteProductCommand"/>.
/// </summary>
public class DeleteProductCommandHandler(CatalogDbContext db) : IRequestHandler<DeleteProductCommand, bool>
{
    /// <inheritdoc />
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync(request.Id, cancellationToken);
        if (product is null) return false;

        product.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
