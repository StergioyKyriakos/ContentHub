using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.AuditLogs.GetAuditLogs;

public sealed class GetAuditLogsQuery
{
    public Guid? ActorUserId { get; set; }

    public AuditAction? Action { get; set; }

    public string? EntityName { get; set; }

    public string? EntityId { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
