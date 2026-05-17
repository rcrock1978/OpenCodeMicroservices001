using MediatR;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Commands;

/// <summary>
/// Command to refund a succeeded payment intent.
/// </summary>
public record RefundPaymentIntentCommand(Guid Id) : IRequest<PaymentIntent?>;

/// <summary>
/// Handles the <see cref="RefundPaymentIntentCommand"/>.
/// </summary>
public class RefundPaymentIntentCommandHandler(PaymentDbContext db) : IRequestHandler<RefundPaymentIntentCommand, PaymentIntent?>
{
    /// <inheritdoc />
    public async Task<PaymentIntent?> Handle(RefundPaymentIntentCommand request, CancellationToken cancellationToken)
    {
        var intent = await db.PaymentIntents.FindAsync([request.Id], cancellationToken);
        if (intent is null)
        {
            return null;
        }

        if (intent.Status != PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Only succeeded payments can be refunded");
        }

        intent.Status = PaymentStatus.Refunded;
        await db.SaveChangesAsync(cancellationToken);
        return intent;
    }
}
