using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging.IntegrationEvents;

namespace CustomerService.Infrastructure.Consumers;

/// <summary>
/// Consumes order placed events and creates denormalized order summaries.
/// </summary>
public class OrderPlacedEventConsumer : IConsumer<OrderPlacedIntegrationEvent>
{
    private readonly CustomerDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderPlacedEventConsumer"/> class.
    /// </summary>
    public OrderPlacedEventConsumer(CustomerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
    {
        var evt = context.Message;

        var summary = new OrderSummary
        {
            Id = Guid.NewGuid(),
            TenantId = evt.TenantId,
            OrderId = evt.OrderId,
            CustomerId = evt.CustomerId,
            TotalAmount = evt.TotalAmount,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.OrderSummaries.Add(summary);
        await _dbContext.SaveChangesAsync();
    }
}
