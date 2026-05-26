using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.AuditLogs;

public sealed class AuditLog : Entity
{
    private readonly List<AuditEntityChange> _changes = [];

    public AuditLog()
    {
    }

    public AuditLog(
        Guid? actorUserId,
        AuditAction action,
        string entityName,
        string? entityId,
        string? oldValuesJson,
        string? newValuesJson,
        string? ipAddress,
        string? userAgent)
    {
        ActorUserId = actorUserId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        OldValuesJson = oldValuesJson;
        NewValuesJson = newValuesJson;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid? ActorUserId { get; set; }

    public AuditAction Action { get; set; }

    public string EntityName { get; set; } = null!;

    public string? EntityId { get; set; }

    public string? OldValuesJson { get; set; }

    public string? NewValuesJson { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public IReadOnlyCollection<AuditEntityChange> Changes => _changes.AsReadOnly();

    public void AddChange(
        string propertyName,
        string? oldValue,
        string? newValue)
    {
        _changes.Add(new AuditEntityChange(
            auditLogId: Id,
            propertyName: propertyName,
            oldValue: oldValue,
            newValue: newValue));
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(auditLog => auditLog.Id);

        builder.Property(auditLog => auditLog.Id)
            .ValueGeneratedNever();

        builder.Property(auditLog => auditLog.ActorUserId);

        builder.Property(auditLog => auditLog.Action)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(auditLog => auditLog.EntityName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(auditLog => auditLog.EntityId)
            .HasMaxLength(100);

        builder.Property(auditLog => auditLog.OldValuesJson)
            .HasColumnType("jsonb");

        builder.Property(auditLog => auditLog.NewValuesJson)
            .HasColumnType("jsonb");

        builder.Property(auditLog => auditLog.IpAddress)
            .HasMaxLength(100);

        builder.Property(auditLog => auditLog.UserAgent)
            .HasMaxLength(500);

        builder.Property(auditLog => auditLog.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(auditLog => auditLog.ActorUserId);

        builder.HasIndex(auditLog => auditLog.Action);

        builder.HasIndex(auditLog => auditLog.EntityName);

        builder.HasIndex(auditLog => auditLog.EntityId);

        builder.HasIndex(auditLog => auditLog.CreatedAtUtc);

        builder.HasMany(auditLog => auditLog.Changes)
            .WithOne(change => change.AuditLog)
            .HasForeignKey(change => change.AuditLogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}