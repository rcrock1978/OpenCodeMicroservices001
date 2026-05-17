using MediatR;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Commands;

/// <summary>
/// Command to create a new payment intent.
/// </summary>
public record CreatePaymentIntentCommand(
    Guid TenantId,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string IdempotencyKey,
    string? PaymentMethod,
    bool TestFailure) : IRequest<PaymentIntent>;

/// <summary>
/// Handles the <see cref="CreatePaymentIntentCommand"/>.
/// </summary>
public class CreatePaymentIntentCommandHandler(PaymentDbContext db) : IRequestHandler<CreatePaymentIntentCommand, PaymentIntent>
{
    /// <inheritdoc />
    public async Task<PaymentIntent> Handle(CreatePaymentIntentCommand request, CancellationToken cancellationToken)
    {
        var status = request.Amount > 0 && !request.TestFailure
            ? PaymentStatus.Succeeded
            : PaymentStatus.Failed;

        var intent = new PaymentIntent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            Currency = request.Currency,
            IdempotencyKey = request.IdempotencyKey,
            Status = status,
            PaymentMethod = request.PaymentMethod,
            FailureReason = status == PaymentStatus.Failed ? "Test failure or zero amount" : null,
            CapturedAt = status == PaymentStatus.Succeeded ? DateTime.UtcNow : null
        };

        db.PaymentIntents.Add(intent);
        await db.SaveChangesAsync(cancellationToken);
        return intent;
    }
}
