using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Queries;

/// <summary>
/// Query to retrieve a payment intent by its identifier.
/// </summary>
public record GetPaymentIntentByIdQuery(Guid Id) : IRequest<PaymentIntent?>;

/// <summary>
/// Handles the <see cref="GetPaymentIntentByIdQuery"/>.
/// </summary>
public class GetPaymentIntentByIdQueryHandler(PaymentDbContext db) : IRequestHandler<GetPaymentIntentByIdQuery, PaymentIntent?>
{
    /// <inheritdoc />
    public async Task<PaymentIntent?> Handle(GetPaymentIntentByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.PaymentIntents.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
    }
}
