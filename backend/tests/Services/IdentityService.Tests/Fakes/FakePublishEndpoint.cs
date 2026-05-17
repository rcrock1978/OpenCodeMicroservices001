using MassTransit;

namespace IdentityService.Tests.Fakes;

/// <summary>
/// Fake implementation of <see cref="IPublishEndpoint"/> that captures published messages for verification.
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
    public Task Publish<T>(T message, IPipe<PublishContext<T>> pipe, CancellationToken cancellationToken = default)
        where T : class
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task Publish<T>(T message, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default)
        where T : class
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task Publish(object message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task Publish(object message, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task Publish(object message, Type messageType, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task Publish<T>(object values, CancellationToken cancellationToken = default)
        where T : class
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task Publish<T>(object values, IPipe<PublishContext<T>> pipe, CancellationToken cancellationToken = default)
        where T : class
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task Publish<T>(object values, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default)
        where T : class
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
    {
        throw new NotImplementedException();
    }
}
