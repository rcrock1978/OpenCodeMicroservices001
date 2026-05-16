using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using OrderService.Infrastructure.Sagas;

namespace PaymentService.Infrastructure.Consumers;

/// <summary>
/// Consumes refund payment commands and processes refunds.
/// </summary>
public class RefundPaymentCommandConsumer : IConsumer<RefundPaymentCommand>
{
    private readonly PaymentDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefundPaymentCommandConsumer"/> class.
    /// </summary>
    public RefundPaymentCommandConsumer(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<RefundPaymentCommand> context)
    {
        var command = context.Message;

        var intent = await _dbContext.PaymentIntents
            .FirstOrDefaultAsync(p => p.OrderId == command.OrderId && p.TenantId == command.TenantId && p.Status == PaymentStatus.Succeeded);

        if (intent is null) return;

        intent.Status = PaymentStatus.Refunded;

        _dbContext.PaymentTransactions.Add(new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            PaymentIntentId = intent.Id,
            Type = PaymentTransactionType.Refund,
            Amount = intent.Amount,
            Status = PaymentTransactionStatus.Succeeded,
            GatewayResponse = "{\"status\": \"refunded\"}"
        });

        await _dbContext.SaveChangesAsync();

        await context.Publish(new PaymentRefundedIntegrationEvent
        {
            OrderId = command.OrderId,
            TenantId = command.TenantId,
            PaymentIntentId = intent.Id,
            Amount = intent.Amount,
            RefundedAt = DateTimeOffset.UtcNow
        });
    }
}
