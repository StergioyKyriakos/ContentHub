using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.AuditLogs.Shared;

public static class AuditLogErrors
{
    public static ApiError NotFound =>
        ApiError.Create(
            code: "audit_logs.not_found",
            message: "Audit log was not found.");
}