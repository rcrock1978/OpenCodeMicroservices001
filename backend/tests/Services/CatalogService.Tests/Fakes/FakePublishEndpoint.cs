using MassTransit;

namespace CatalogService.Tests.Fakes;

/// <summary>
/// A fake implementation of <see cref="IPublishEndpoint"/> that captures published messages for testing.
/// </summary>
public class FakePublishEndpoint : IPublishEndpoint
{
    /// <summary>
    /// Gets the list of messages that have been published.
    /// </summary>
    public List<object> PublishedMessages { get; } = [];

    /// <inheritdoc />
    public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish(object message, CancellationToken cancellationToken = default)
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(object message, CancellationToken cancellationToken = default) where T : class
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(object message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
    {
        return new FakeConnectHandle();
    }

    private class FakeConnectHandle : ConnectHandle
    {
        public void Disconnect() { }
        public void Dispose() { }
    }
}
