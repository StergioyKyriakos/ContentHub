namespace ContentHub.Data.Entities.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public bool IsDeleted { get; set; }

    protected AuditableEntity() {}

    protected AuditableEntity(Guid id) : base(id) { }

    public void MarkAsUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        MarkAsUpdated();
    }
}