using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Application.Queries;

/// <summary>
/// Query to retrieve a single order by identifier.
/// </summary>
public record GetOrderByIdQuery(Guid Id) : IRequest<Order?>;

/// <summary>
/// Handler for <see cref="GetOrderByIdQuery"/>.
/// </summary>
public class GetOrderByIdQueryHandler(OrderDbContext db) : IRequestHandler<GetOrderByIdQuery, Order?>
{
    /// <inheritdoc />
    public async Task<Order?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
    }
}
