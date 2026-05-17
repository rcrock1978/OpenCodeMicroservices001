using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Application.Queries;

/// <summary>
/// Query to retrieve all orders.
/// </summary>
public record GetOrdersQuery : IRequest<List<Order>>;

/// <summary>
/// Handler for <see cref="GetOrdersQuery"/>.
/// </summary>
public class GetOrdersQueryHandler(OrderDbContext db) : IRequestHandler<GetOrdersQuery, List<Order>>
{
    /// <inheritdoc />
    public async Task<List<Order>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        return await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);
    }
}
