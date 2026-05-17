using MassTransit;

namespace OrderService.Tests.Fakes;

/// <summary>
/// A fake implementation of <see cref="IPublishEndpoint"/> that captures published messages
/// for unit test assertions.
/// </summary>
public class FakePublishEndpoint : IPublishEndpoint
{
    private readonly List<object> _publishedMessages = [];

    /// <summary>
    /// Gets the collection of messages published through this endpoint.
    /// </summary>
    public IReadOnlyList<object> PublishedMessages => _publishedMessages.AsReadOnly();

    /// <summary>
    /// Gets the first published message of type <typeparamref name="T"/>.
    /// </summary>
    public T? GetPublishedMessage<T>() where T : class
    {
        return _publishedMessages.OfType<T>().FirstOrDefault();
    }

    /// <inheritdoc />
    public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
    {
        return new NullConnectHandle();
    }

    /// <inheritdoc />
    public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        if (message is not null)
        {
            _publishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
    {
        if (message is not null)
        {
            _publishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
    {
        if (message is not null)
        {
            _publishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish(object message, CancellationToken cancellationToken = default)
    {
        if (message is not null)
        {
            _publishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
    {
        if (message is not null)
        {
            _publishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
    {
        if (message is not null)
        {
            _publishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
    {
        if (message is not null)
        {
            _publishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(object values, CancellationToken cancellationToken = default) where T : class
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
    {
        return Task.CompletedTask;
    }

    private class NullConnectHandle : ConnectHandle
    {
        public void Disconnect()
        {
            // No-op for fake implementation.
        }

        public void Dispose()
        {
            // No-op for fake implementation.
        }
    }
}
