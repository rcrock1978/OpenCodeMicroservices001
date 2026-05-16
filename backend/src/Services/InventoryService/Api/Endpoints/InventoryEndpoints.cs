using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Api.Endpoints;

/// <summary>
/// API endpoints for inventory management.
/// </summary>
public static class InventoryEndpoints
{
    /// <summary>
    /// Maps inventory-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").WithTags("Inventory").WithOpenApi();

        group.MapGet("/", async (InventoryDbContext db) =>
            Results.Ok(await db.StockItems.AsNoTracking().ToListAsync()));

        group.MapGet("/{id:guid}", async (Guid id, InventoryDbContext db) =>
            await db.StockItems.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id) is StockItem item
                ? Results.Ok(item)
                : Results.NotFound());

        group.MapPost("/", async (CreateStockItemRequest request, InventoryDbContext db) =>
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
            await db.SaveChangesAsync();
            return Results.Created($"/api/inventory/{item.Id}", item);
        });

        group.MapPost("/reserve", async (ReserveStockRequest request, InventoryDbContext db) =>
        {
            var item = await db.StockItems.FirstOrDefaultAsync(s => s.Sku == request.Sku && s.TenantId == request.TenantId);
            if (item is null) return Results.NotFound("Stock item not found");
            if (item.QuantityAvailable - item.QuantityReserved < request.Quantity)
                return Results.BadRequest("Insufficient stock");

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
            await db.SaveChangesAsync();
            return Results.Ok(reservation);
        });

        group.MapPost("/release", async (ReleaseStockRequest request, InventoryDbContext db) =>
        {
            var reservation = await db.StockReservations
                .Include(r => r.StockItemId)
                .FirstOrDefaultAsync(r => r.OrderId == request.OrderId && r.TenantId == request.TenantId && r.Status == ReservationStatus.Reserved);

            if (reservation is null) return Results.NotFound("Reservation not found");

            var item = await db.StockItems.FindAsync(reservation.StockItemId);
            if (item is not null) item.QuantityReserved -= reservation.Quantity;
            reservation.Status = ReservationStatus.Released;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a stock item.
/// </summary>
public record CreateStockItemRequest(Guid TenantId, Guid ProductVariantId, string Sku, int QuantityAvailable, int LowStockThreshold);

/// <summary>
/// Request model for reserving stock.
/// </summary>
public record ReserveStockRequest(Guid TenantId, string Sku, Guid OrderId, int Quantity);

/// <summary>
/// Request model for releasing stock.
/// </summary>
public record ReleaseStockRequest(Guid TenantId, Guid OrderId);
