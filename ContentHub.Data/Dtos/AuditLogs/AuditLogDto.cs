using ContentHub.Data.Enums;

namespace ContentHub.Data.Dtos.AuditLogs;

public sealed class AuditLogDto
{
    public Guid Id { get; set; }

    public Guid? ActorUserId { get; set; }

    public AuditAction Action { get; set; }

    public string ActionName { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public string? EntityId { get; set; }

    public string? OldValuesJson { get; set; }

    public string? NewValuesJson { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public IReadOnlyCollection<AuditEntityChangeDto> Changes { get; set; } = [];
}

