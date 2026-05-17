using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Consumers;
using InventoryService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Messaging;
using Xunit;

namespace InventoryService.Tests.Infrastructure.Consumers;

/// <summary>
/// Unit tests for the <see cref="InventoryReleaseCommandConsumer"/>.
/// </summary>
public class InventoryReleaseCommandConsumerTests
{
    private static InventoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InventoryDbContext(options);
    }

    [Fact]
    public async Task Consume_WithReservedStock_ShouldReleaseAndCreateMovement()
    {
        var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var productVariantId = Guid.NewGuid();

        db.StockItems.Add(new StockItem
        {
            Id = itemId,
            TenantId = tenantId,
            ProductVariantId = productVariantId,
            Sku = "RELEASE-001",
            QuantityAvailable = 100,
            QuantityReserved = 10
        });

        db.StockReservations.Add(new StockReservation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderId = orderId,
            StockItemId = itemId,
            Quantity = 10,
            Status = ReservationStatus.Reserved,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });

        await db.SaveChangesAsync();

        var consumer = new InventoryReleaseCommandConsumer(db);
        var command = new InventoryReleaseCommand
        {
            OrderId = orderId,
            TenantId = tenantId
        };

        await consumer.Consume(new FakeConsumeContext<InventoryReleaseCommand>(command));

        var item = await db.StockItems.FindAsync(itemId);
        var reservation = await db.StockReservations
            .FirstOrDefaultAsync(r => r.OrderId == orderId && r.TenantId == tenantId);
        var movement = await db.StockMovements
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Type == StockMovementType.Release);

        Assert.NotNull(item);
        Assert.Equal(0, item.QuantityReserved);
        Assert.NotNull(reservation);
        Assert.Equal(ReservationStatus.Released, reservation.Status);
        Assert.NotNull(movement);
        Assert.Equal(StockMovementType.Release, movement.Type);
        Assert.Equal(10, movement.Quantity);
    }

    [Fact]
    public async Task Consume_WithNoReservations_ShouldDoNothing()
    {
        var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var consumer = new InventoryReleaseCommandConsumer(db);
        var command = new InventoryReleaseCommand
        {
            OrderId = orderId,
            TenantId = tenantId
        };

        await consumer.Consume(new FakeConsumeContext<InventoryReleaseCommand>(command));

        Assert.Empty(db.StockMovements);
    }

    [Fact]
    public async Task Consume_MultipleReservations_ShouldReleaseAll()
    {
        var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();

        db.StockItems.AddRange(
            new StockItem { Id = itemId1, TenantId = tenantId, ProductVariantId = Guid.NewGuid(), Sku = "MULTI-1", QuantityAvailable = 50, QuantityReserved = 5 },
            new StockItem { Id = itemId2, TenantId = tenantId, ProductVariantId = Guid.NewGuid(), Sku = "MULTI-2", QuantityAvailable = 50, QuantityReserved = 3 }
        );

        db.StockReservations.AddRange(
            new StockReservation { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = orderId, StockItemId = itemId1, Quantity = 5, Status = ReservationStatus.Reserved, ExpiresAt = DateTime.UtcNow.AddMinutes(15) },
            new StockReservation { Id = Guid.NewGuid(), TenantId = tenantId, OrderId = orderId, StockItemId = itemId2, Quantity = 3, Status = ReservationStatus.Reserved, ExpiresAt = DateTime.UtcNow.AddMinutes(15) }
        );

        await db.SaveChangesAsync();

        var consumer = new InventoryReleaseCommandConsumer(db);
        var command = new InventoryReleaseCommand { OrderId = orderId, TenantId = tenantId };

        await consumer.Consume(new FakeConsumeContext<InventoryReleaseCommand>(command));

        var item1 = await db.StockItems.FindAsync(itemId1);
        var item2 = await db.StockItems.FindAsync(itemId2);

        Assert.Equal(0, item1!.QuantityReserved);
        Assert.Equal(0, item2!.QuantityReserved);
        Assert.Equal(2, await db.StockMovements.CountAsync());
    }
}

/// <summary>
/// Minimal fake ConsumeContext for testing consumers that only use Message.
/// </summary>
#pragma warning disable CS8767
public class FakeConsumeContext<T> : ConsumeContext<T> where T : class
{
    private readonly T _message;

    public FakeConsumeContext(T message) => _message = message;

    public T Message => _message;
    public Guid? MessageId => Guid.NewGuid();
    public Guid? RequestId => null;
    public Guid? CorrelationId => null;
    public Guid? ConversationId => null;
    public Guid? InitiatorId => null;
    public DateTime? ExpirationTime => null;
    public Uri? SourceAddress => null;
    public Uri? DestinationAddress => null;
    public Uri? ResponseAddress => null;
    public Uri? FaultAddress => null;
    public DateTime? SentTime => DateTime.UtcNow;
    public Headers Headers => throw new NotImplementedException();
    public HostInfo Host => throw new NotImplementedException();
    public IEnumerable<string> SupportedMessageTypes => Array.Empty<string>();
    public bool HasMessageType(Type messageType) => messageType == typeof(T);
    public bool TryGetMessage<TMessage>(out ConsumeContext<TMessage> consumeContext) where TMessage : class { consumeContext = null!; return false; }
    public void Respond<TMessage>(TMessage message) where TMessage : class { }
    public Task RespondAsync<TMessage>(TMessage message) where TMessage : class => Task.CompletedTask;
    public Task RespondAsync<TMessage>(TMessage message, IPipe<SendContext<TMessage>> sendPipe) where TMessage : class => Task.CompletedTask;
    public Task RespondAsync<TMessage>(TMessage message, IPipe<SendContext> sendPipe) where TMessage : class => Task.CompletedTask;
    public Task NotifyConsumed(TimeSpan duration, string consumerType) => Task.CompletedTask;
    public Task NotifyFaulted(TimeSpan duration, string consumerType, Exception exception) => Task.CompletedTask;
    public CancellationToken CancellationToken => CancellationToken.None;
    public ConsumeContext BaseContext => this;
    public ReceiveContext ReceiveContext => throw new NotImplementedException();
    public IServiceProvider ServiceProvider => new FakeServiceProvider();
    public IPublishEndpoint PublishEndpoint => throw new NotImplementedException();
    public Task Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class => Task.CompletedTask;
    public Task Publish<TMessage>(TMessage message, IPipe<PublishContext<TMessage>> publishPipe, CancellationToken cancellationToken = default) where TMessage : class => Task.CompletedTask;
    public Task Publish<TMessage>(TMessage message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where TMessage : class => Task.CompletedTask;
    public Task Publish(object message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Publish<TPayload>(object message, CancellationToken cancellationToken = default) where TPayload : class => Task.CompletedTask;
    public Task Publish<TPayload>(object message, IPipe<PublishContext<TPayload>> publishPipe, CancellationToken cancellationToken = default) where TPayload : class => Task.CompletedTask;
    public Task Publish<TPayload>(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where TPayload : class => Task.CompletedTask;
    public TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory) where TPayload : class => throw new NotImplementedException();
    public bool HasPayloadType(Type payloadType) => false;
    public bool TryGetPayload<TPayload>(out TPayload payload) where TPayload : class { payload = default!; return false; }
    public void AddConsumeTask(Task task) { }
    public Task RespondAsync(object message) => Task.CompletedTask;
    public Task RespondAsync(object message, Type messageType) => Task.CompletedTask;
    public Task RespondAsync(object message, IPipe<SendContext> sendPipe) => Task.CompletedTask;
    public Task RespondAsync(object message, Type messageType, IPipe<SendContext> sendPipe) => Task.CompletedTask;
    public Task RespondAsync<TPayload>(object message) where TPayload : class => Task.CompletedTask;
    public Task RespondAsync<TPayload>(object message, IPipe<SendContext<TPayload>> sendPipe) where TPayload : class => Task.CompletedTask;
    public Task RespondAsync<TPayload>(object message, IPipe<SendContext> sendPipe) where TPayload : class => Task.CompletedTask;
    public Task NotifyConsumed<TPayload>(ConsumeContext<TPayload> context, TimeSpan duration, string consumerType) where TPayload : class => Task.CompletedTask;
    public Task NotifyFaulted<TPayload>(ConsumeContext<TPayload> context, TimeSpan duration, string consumerType, Exception exception) where TPayload : class => Task.CompletedTask;
    public SerializerContext SerializerContext => throw new NotImplementedException();
    public Task ConsumeCompleted => Task.CompletedTask;
    public TPayload AddOrUpdatePayload<TPayload>(PayloadFactory<TPayload> addFactory, UpdatePayloadFactory<TPayload> updateFactory) where TPayload : class => throw new NotImplementedException();
    public Task<ISendEndpoint> GetSendEndpoint(Uri address) => throw new NotImplementedException();
    public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => throw new NotImplementedException();
    public ConnectHandle ConnectSendObserver(ISendObserver observer) => throw new NotImplementedException();
}
#pragma warning restore CS8767

public class FakeServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType) => null;
}
