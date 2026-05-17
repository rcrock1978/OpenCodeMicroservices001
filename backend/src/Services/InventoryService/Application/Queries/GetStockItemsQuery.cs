using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Queries;

/// <summary>
/// Query to retrieve all stock items.
/// </summary>
public record GetStockItemsQuery : IRequest<List<StockItem>>;

/// <summary>
/// Handles the <see cref="GetStockItemsQuery"/>.
/// </summary>
public class GetStockItemsQueryHandler(InventoryDbContext db) : IRequestHandler<GetStockItemsQuery, List<StockItem>>
{
    /// <inheritdoc />
    public async Task<List<StockItem>> Handle(GetStockItemsQuery request, CancellationToken cancellationToken)
    {
        return await db.StockItems.AsNoTracking().ToListAsync(cancellationToken);
    }
}
