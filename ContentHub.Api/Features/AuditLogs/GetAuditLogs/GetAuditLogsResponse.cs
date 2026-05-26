using ContentHub.Data.Dtos.AuditLogs;
using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.AuditLogs.GetAuditLogs;

public sealed class GetAuditLogsResponse
{
    public PagedResponse<AuditLogDto> AuditLogs { get; set; } = null!;
}