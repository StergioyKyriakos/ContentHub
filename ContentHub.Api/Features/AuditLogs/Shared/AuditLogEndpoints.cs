namespace ContentHub.Api.Features.AuditLogs.Shared;

public static class AuditLogEndpoints
{
    public const string GetAll = "/api/admin/audit-logs";

    public const string GetById = "/api/admin/audit-logs/{id:guid}";

    public const string Export = "/api/admin/audit-logs/export";
}