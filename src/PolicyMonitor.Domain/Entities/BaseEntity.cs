namespace PolicyMonitor.Domain.Entities;

/// <summary>
/// Base class for all domain entities providing common audit fields.
/// </summary>
public abstract class BaseEntity<TKey> where TKey : notnull
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public TKey Id { get; set; } = default!;

    /// <summary>
    /// UTC timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User or system that created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = "System";

    /// <summary>
    /// User or system that last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag for logical deletion.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// UTC timestamp when the entity was soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User or system that deleted the entity.
    /// </summary>
    public string? DeletedBy { get; set; }
}
