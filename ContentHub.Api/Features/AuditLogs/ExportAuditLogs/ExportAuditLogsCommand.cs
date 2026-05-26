using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.AuditLogs.ExportAuditLogs;

public sealed class ExportAuditLogsCommand
{
    public Guid? ActorUserId { get; set; }

    public AuditAction? Action { get; set; }

    public string? EntityName { get; set; }

    public string? EntityId { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }
}