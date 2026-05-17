using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Queries;

/// <summary>
/// Query to retrieve all payment methods.
/// </summary>
public record GetPaymentMethodsQuery : IRequest<List<PaymentMethod>>;

/// <summary>
/// Handles the <see cref="GetPaymentMethodsQuery"/>.
/// </summary>
public class GetPaymentMethodsQueryHandler(PaymentDbContext db) : IRequestHandler<GetPaymentMethodsQuery, List<PaymentMethod>>
{
    /// <inheritdoc />
    public async Task<List<PaymentMethod>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        return await db.PaymentMethods.AsNoTracking().ToListAsync(cancellationToken);
    }
}
