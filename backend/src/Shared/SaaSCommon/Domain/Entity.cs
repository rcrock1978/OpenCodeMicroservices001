namespace SaaSCommon.Domain;

/// <summary>
/// Base entity class for all domain entities with tenant isolation.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the tenant identifier for row-level tenant isolation.
    /// </summary>
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Gets or sets when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
}
