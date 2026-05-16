using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using SaaSCommon.Messaging.IntegrationEvents;

namespace PaymentService.Infrastructure.Consumers;

/// <summary>
/// Consumes payment initiate commands and processes payment simulation.
/// </summary>
public class PaymentInitiateCommandConsumer : IConsumer<OrderService.Infrastructure.Sagas.PaymentInitiateCommand>
{
    private readonly PaymentDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentInitiateCommandConsumer"/> class.
    /// </summary>
    public PaymentInitiateCommandConsumer(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<OrderService.Infrastructure.Sagas.PaymentInitiateCommand> context)
    {
        var command = context.Message;

        // Simulate payment processing: success if amount > 0
        var succeeded = command.Amount > 0;

        var intent = new PaymentIntent
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            OrderId = command.OrderId,
            CustomerId = Guid.Empty,
            Amount = command.Amount,
            Currency = "USD",
            IdempotencyKey = Guid.NewGuid().ToString(),
            Status = succeeded ? PaymentStatus.Succeeded : PaymentStatus.Failed,
            FailureReason = succeeded ? null : "Test failure or zero amount",
            CapturedAt = succeeded ? DateTime.UtcNow : null
        };

        _dbContext.PaymentIntents.Add(intent);

        _dbContext.PaymentTransactions.Add(new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            PaymentIntentId = intent.Id,
            Type = PaymentTransactionType.Authorization,
            Amount = command.Amount,
            Status = succeeded ? PaymentTransactionStatus.Succeeded : PaymentTransactionStatus.Failed,
            GatewayResponse = succeeded ? "{\"status\": \"ok\"}" : "{\"status\": \"failed\"}"
        });

        await _dbContext.SaveChangesAsync();

        if (succeeded)
        {
            await context.Publish(new PaymentProcessedIntegrationEvent
            {
                PaymentIntentId = intent.Id,
                OrderId = command.OrderId,
                TenantId = command.TenantId,
                Amount = command.Amount,
                ProcessedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            await context.Publish(new PaymentFailedIntegrationEvent
            {
                PaymentIntentId = intent.Id,
                OrderId = command.OrderId,
                TenantId = command.TenantId,
                FailureReason = intent.FailureReason!
            });
        }
    }
}
