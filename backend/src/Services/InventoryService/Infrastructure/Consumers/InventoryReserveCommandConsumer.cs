using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging;
using SaaSCommon.Messaging.IntegrationEvents;

namespace InventoryService.Infrastructure.Consumers;

/// <summary>
/// Consumes inventory reserve commands and publishes reserve success/failure events.
/// </summary>
public class InventoryReserveCommandConsumer : IConsumer<InventoryReserveCommand>
{
    private readonly InventoryDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryReserveCommandConsumer"/> class.
    /// </summary>
    public InventoryReserveCommandConsumer(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<InventoryReserveCommand> context)
    {
        var command = context.Message;
        var reservedQuantities = new Dictionary<Guid, int>();

        foreach (var item in command.Items)
        {
            var stockItem = await _dbContext.StockItems
                .FirstOrDefaultAsync(s => s.ProductVariantId == item.Key && s.TenantId == command.TenantId);

            if (stockItem is null || stockItem.QuantityAvailable - stockItem.QuantityReserved < item.Value)
            {
                await context.Publish(new InventoryReservationFailedIntegrationEvent
                {
                    OrderId = command.OrderId,
                    TenantId = command.TenantId,
                    Reason = $"Insufficient stock for product variant {item.Key}"
                });
                return;
            }

            stockItem.QuantityReserved += item.Value;
            reservedQuantities[item.Key] = item.Value;

            var reservation = new StockReservation
            {
                Id = Guid.NewGuid(),
                TenantId = command.TenantId,
                OrderId = command.OrderId,
                StockItemId = stockItem.Id,
                Quantity = item.Value,
                Status = ReservationStatus.Reserved,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            _dbContext.StockReservations.Add(reservation);

            _dbContext.StockMovements.Add(new StockMovement
            {
                Id = Guid.NewGuid(),
                TenantId = command.TenantId,
                StockItemId = stockItem.Id,
                Type = StockMovementType.Reservation,
                Quantity = item.Value,
                Reference = $"Order {command.OrderId}"
            });
        }

        await _dbContext.SaveChangesAsync();

        await context.Publish(new InventoryReservedIntegrationEvent
        {
            OrderId = command.OrderId,
            TenantId = command.TenantId,
            ReservedQuantities = reservedQuantities
        });
    }
}
