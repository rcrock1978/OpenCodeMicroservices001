using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Queries;

/// <summary>
/// Query to retrieve all payment intents.
/// </summary>
public record GetPaymentIntentsQuery : IRequest<List<PaymentIntent>>;

/// <summary>
/// Handles the <see cref="GetPaymentIntentsQuery"/>.
/// </summary>
public class GetPaymentIntentsQueryHandler(PaymentDbContext db) : IRequestHandler<GetPaymentIntentsQuery, List<PaymentIntent>>
{
    /// <inheritdoc />
    public async Task<List<PaymentIntent>> Handle(GetPaymentIntentsQuery request, CancellationToken cancellationToken)
    {
        return await db.PaymentIntents.AsNoTracking().ToListAsync(cancellationToken);
    }
}
