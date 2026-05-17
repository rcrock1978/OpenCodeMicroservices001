using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using MediatR;

namespace InventoryService.Application.Commands;

/// <summary>
/// Command to create a new stock item.
/// </summary>
public record CreateStockItemCommand(
    Guid TenantId,
    Guid ProductVariantId,
    string Sku,
    int QuantityAvailable,
    int LowStockThreshold) : IRequest<StockItem>;

/// <summary>
/// Handles the <see cref="CreateStockItemCommand"/>.
/// </summary>
public class CreateStockItemCommandHandler(InventoryDbContext db) : IRequestHandler<CreateStockItemCommand, StockItem>
{
    /// <inheritdoc />
    public async Task<StockItem> Handle(CreateStockItemCommand request, CancellationToken cancellationToken)
    {
        var item = new StockItem
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ProductVariantId = request.ProductVariantId,
            Sku = request.Sku,
            QuantityAvailable = request.QuantityAvailable,
            QuantityReserved = 0,
            LowStockThreshold = request.LowStockThreshold
        };

        db.StockItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return item;
    }
}
