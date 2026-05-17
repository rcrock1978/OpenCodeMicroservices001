using MediatR;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Commands;

/// <summary>
/// Command to create a new payment method.
/// </summary>
public record CreatePaymentMethodCommand(
    Guid TenantId,
    Guid CustomerId,
    PaymentMethodType Type,
    string? LastFour,
    string? Brand,
    int? ExpMonth,
    int? ExpYear,
    bool IsDefault) : IRequest<PaymentMethod>;

/// <summary>
/// Handles the <see cref="CreatePaymentMethodCommand"/>.
/// </summary>
public class CreatePaymentMethodCommandHandler(PaymentDbContext db) : IRequestHandler<CreatePaymentMethodCommand, PaymentMethod>
{
    /// <inheritdoc />
    public async Task<PaymentMethod> Handle(CreatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            CustomerId = request.CustomerId,
            Type = request.Type,
            LastFour = request.LastFour,
            Brand = request.Brand,
            ExpMonth = request.ExpMonth,
            ExpYear = request.ExpYear,
            IsDefault = request.IsDefault
        };

        db.PaymentMethods.Add(method);
        await db.SaveChangesAsync(cancellationToken);
        return method;
    }
}
