using MassTransit;

namespace CustomerService.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IPublishEndpoint"/> that captures published messages in memory.
/// </summary>
public class FakePublishEndpoint : IPublishEndpoint
{
    /// <summary>
    /// Gets the list of messages published through this endpoint.
    /// </summary>
    public List<object> PublishedMessages { get; } = new();

    /// <inheritdoc />
    public Task Publish<T>(T message, CancellationToken cancellationToken = default)
        where T : class
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default)
        where T : class
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        where T : class
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
    public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
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
    public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(object message, CancellationToken cancellationToken = default)
        where T : class
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(object message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default)
        where T : class
    {
        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Publish<T>(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        where T : class
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
        throw new NotImplementedException();
    }
}
