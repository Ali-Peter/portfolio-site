namespace Portfolio.Domain.Entities;

/// <summary>
/// Base type for every persistent entity: identity, audit timestamps and soft delete.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>UserId or system name ("seed", "system") of the creator.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>UserId or system name of the last modifier.</summary>
    public string? UpdatedBy { get; set; }

    /// <summary>Soft-delete flag; enforced by a global EF Core query filter.</summary>
    public bool IsDeleted { get; set; }
}
