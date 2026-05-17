namespace SaaSCommon.Messaging.IntegrationEvents;

/// <summary>
/// Published when a new product is created.
/// </summary>
public record ProductCreatedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(ProductCreatedIntegrationEvent);

    public Guid ProductId { get; init; }

    public string Name { get; init; } = null!;
    public string Sku { get; init; } = null!;
    public decimal Price { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Published when a product is updated.
/// </summary>
public record ProductUpdatedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(ProductUpdatedIntegrationEvent);

    public Guid ProductId { get; init; }

    public string Name { get; init; } = null!;
    public string Sku { get; init; } = null!;
    public decimal Price { get; init; }
}

/// <summary>
/// Published when a product is deleted.
/// </summary>
public record ProductDeletedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(ProductDeletedIntegrationEvent);

    public Guid ProductId { get; init; }
}

/// <summary>
/// Published when a new category is created.
/// </summary>
public record CategoryCreatedIntegrationEvent : IntegrationEvent
{
    public override string EventType => nameof(CategoryCreatedIntegrationEvent);

    public Guid CategoryId { get; init; }

    public string Name { get; init; } = null!;
    public Guid? ParentCategoryId { get; init; }
}
