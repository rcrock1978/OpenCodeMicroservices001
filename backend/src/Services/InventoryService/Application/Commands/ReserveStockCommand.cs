using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Commands;

/// <summary>
/// Command to reserve stock for an order.
/// </summary>
public record ReserveStockCommand(
    Guid TenantId,
    string Sku,
    Guid OrderId,
    int Quantity) : IRequest<ReserveStockResult>;

/// <summary>
/// Represents the outcome of a <see cref="ReserveStockCommand"/>.
/// </summary>
public record ReserveStockResult(StockReservation? Reservation, ReserveStockStatus Status);

/// <summary>
/// Defines the possible outcomes of a stock reservation attempt.
/// </summary>
public enum ReserveStockStatus
{
    Success,
    StockItemNotFound,
    InsufficientStock
}

/// <summary>
/// Handles the <see cref="ReserveStockCommand"/>.
/// </summary>
public class ReserveStockCommandHandler(InventoryDbContext db) : IRequestHandler<ReserveStockCommand, ReserveStockResult>
{
    /// <inheritdoc />
    public async Task<ReserveStockResult> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var item = await db.StockItems.FirstOrDefaultAsync(
            s => s.Sku == request.Sku && s.TenantId == request.TenantId,
            cancellationToken);

        if (item is null)
        {
            return new ReserveStockResult(null, ReserveStockStatus.StockItemNotFound);
        }

        if (item.QuantityAvailable - item.QuantityReserved < request.Quantity)
        {
            return new ReserveStockResult(null, ReserveStockStatus.InsufficientStock);
        }

        item.QuantityReserved += request.Quantity;

        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            OrderId = request.OrderId,
            StockItemId = item.Id,
            Quantity = request.Quantity,
            Status = ReservationStatus.Reserved,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        db.StockReservations.Add(reservation);
        await db.SaveChangesAsync(cancellationToken);

        return new ReserveStockResult(reservation, ReserveStockStatus.Success);
    }
}
