using ContentHub.Data.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.AuditLogs;

public sealed class AuditEntityChange : Entity
{
    private AuditEntityChange()
    {
    }

    public AuditEntityChange(
        Guid auditLogId,
        string propertyName,
        string? oldValue,
        string? newValue)
    {
        AuditLogId = auditLogId;
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public Guid AuditLogId { get; set; }

    public string PropertyName { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public AuditLog AuditLog { get; set; } = null!;
}

public sealed class AuditEntityChangeConfiguration : IEntityTypeConfiguration<AuditEntityChange>
{
    public void Configure(EntityTypeBuilder<AuditEntityChange> builder)
    {
        builder.ToTable("audit_entity_changes");

        builder.HasKey(change => change.Id);

        builder.Property(change => change.Id)
            .ValueGeneratedNever();

        builder.Property(change => change.AuditLogId)
            .IsRequired();

        builder.Property(change => change.PropertyName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(change => change.OldValue)
            .HasMaxLength(4000);

        builder.Property(change => change.NewValue)
            .HasMaxLength(4000);

        builder.HasIndex(change => change.AuditLogId);
    }
}