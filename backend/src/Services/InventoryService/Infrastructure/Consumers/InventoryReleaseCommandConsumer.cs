using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging;

namespace InventoryService.Infrastructure.Consumers;

/// <summary>
/// Consumes inventory release commands and releases reserved stock.
/// </summary>
public class InventoryReleaseCommandConsumer : IConsumer<InventoryReleaseCommand>
{
    private readonly InventoryDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryReleaseCommandConsumer"/> class.
    /// </summary>
    public InventoryReleaseCommandConsumer(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<InventoryReleaseCommand> context)
    {
        var command = context.Message;
        var reservations = await _dbContext.StockReservations
            .Where(r => r.OrderId == command.OrderId && r.TenantId == command.TenantId && r.Status == ReservationStatus.Reserved)
            .ToListAsync();

        var releasedQuantities = new Dictionary<Guid, int>();

        foreach (var reservation in reservations)
        {
            var item = await _dbContext.StockItems.FindAsync(reservation.StockItemId);
            if (item is not null)
            {
                item.QuantityReserved -= reservation.Quantity;
                releasedQuantities[item.ProductVariantId] = reservation.Quantity;

                _dbContext.StockMovements.Add(new StockMovement
                {
                    Id = Guid.NewGuid(),
                    TenantId = command.TenantId,
                    StockItemId = item.Id,
                    Type = StockMovementType.Release,
                    Quantity = reservation.Quantity,
                    Reference = $"Order {command.OrderId} cancelled"
                });
            }
            reservation.Status = ReservationStatus.Released;
        }

        await _dbContext.SaveChangesAsync();
    }
}
