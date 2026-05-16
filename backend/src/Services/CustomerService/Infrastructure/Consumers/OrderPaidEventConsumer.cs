using CustomerService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging.IntegrationEvents;

namespace CustomerService.Infrastructure.Consumers;

/// <summary>
/// Consumes order paid events and updates order summary status.
/// </summary>
public class OrderPaidEventConsumer : IConsumer<OrderPaidIntegrationEvent>
{
    private readonly CustomerDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderPaidEventConsumer"/> class.
    /// </summary>
    public OrderPaidEventConsumer(CustomerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<OrderPaidIntegrationEvent> context)
    {
        var evt = context.Message;

        var summary = await _dbContext.OrderSummaries
            .FirstOrDefaultAsync(o => o.OrderId == evt.OrderId && o.TenantId == evt.TenantId);

        if (summary is not null)
        {
            summary.Status = "Paid";
            await _dbContext.SaveChangesAsync();
        }
    }
}
