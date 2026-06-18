namespace ContentHub.Api.Features.AuditLogs.ExportAuditLogs;

public class ExportAuditLogsResponse
{
    public IReadOnlyCollection<ExportAuditLogItemResponse> Items { get; set; } = [];
}

public sealed class ExportAuditLogItemResponse
{
    public Guid Id { get; set; }

    public Guid? ActorUserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string? OldValuesJson { get; set; }

    public string? NewValuesJson { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
