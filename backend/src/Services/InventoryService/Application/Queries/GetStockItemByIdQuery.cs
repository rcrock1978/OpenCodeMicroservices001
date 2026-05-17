using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Queries;

/// <summary>
/// Query to retrieve a stock item by unique identifier.
/// </summary>
public record GetStockItemByIdQuery(Guid Id) : IRequest<StockItem?>;

/// <summary>
/// Handles the <see cref="GetStockItemByIdQuery"/>.
/// </summary>
public class GetStockItemByIdQueryHandler(InventoryDbContext db) : IRequestHandler<GetStockItemByIdQuery, StockItem?>
{
    /// <inheritdoc />
    public async Task<StockItem?> Handle(GetStockItemByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.StockItems.AsNoTracking().FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
    }
}
