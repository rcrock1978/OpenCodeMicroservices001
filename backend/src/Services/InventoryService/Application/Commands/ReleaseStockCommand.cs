using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Commands;

/// <summary>
/// Command to release reserved stock for an order.
/// </summary>
public record ReleaseStockCommand(
    Guid TenantId,
    Guid OrderId) : IRequest<ReleaseStockResult>;

/// <summary>
/// Represents the outcome of a <see cref="ReleaseStockCommand"/>.
/// </summary>
public record ReleaseStockResult(bool Released);

/// <summary>
/// Handles the <see cref="ReleaseStockCommand"/>.
/// </summary>
public class ReleaseStockCommandHandler(InventoryDbContext db) : IRequestHandler<ReleaseStockCommand, ReleaseStockResult>
{
    /// <inheritdoc />
    public async Task<ReleaseStockResult> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        var reservation = await db.StockReservations
            .FirstOrDefaultAsync(
                r => r.OrderId == request.OrderId &&
                     r.TenantId == request.TenantId &&
                     r.Status == ReservationStatus.Reserved,
                cancellationToken);

        if (reservation is null)
        {
            return new ReleaseStockResult(false);
        }

        var item = await db.StockItems.FindAsync(new object[] { reservation.StockItemId }, cancellationToken);
        if (item is not null)
        {
            item.QuantityReserved -= reservation.Quantity;
        }

        reservation.Status = ReservationStatus.Released;
        await db.SaveChangesAsync(cancellationToken);

        return new ReleaseStockResult(true);
    }
}
