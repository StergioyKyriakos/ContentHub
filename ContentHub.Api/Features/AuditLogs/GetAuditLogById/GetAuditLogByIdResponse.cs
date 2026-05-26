using ContentHub.Data.Dtos.AuditLogs;

namespace ContentHub.Api.Features.AuditLogs.GetAuditLogById;

public sealed class GetAuditLogByIdResponse
{
    public AuditLogDto AuditLog { get; set; } = null!;
}