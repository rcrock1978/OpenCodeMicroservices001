using InventoryService.Application.Commands;
using InventoryService.Application.Queries;
using MediatR;

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

        group.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new GetStockItemsQuery(), cancellationToken)));

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var item = await mediator.Send(new GetStockItemByIdQuery(id), cancellationToken);
            return item is not null ? Results.Ok(item) : Results.NotFound();
        });

        group.MapPost("/", async (CreateStockItemCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var item = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/inventory/{item.Id}", item);
        });

        group.MapPost("/reserve", async (ReserveStockCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return result.Status switch
            {
                ReserveStockStatus.StockItemNotFound => Results.NotFound("Stock item not found"),
                ReserveStockStatus.InsufficientStock => Results.BadRequest("Insufficient stock"),
                _ => Results.Ok(result.Reservation)
            };
        });

        group.MapPost("/release", async (ReleaseStockCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return result.Released ? Results.NoContent() : Results.NotFound("Reservation not found");
        });

        return app;
    }
}
