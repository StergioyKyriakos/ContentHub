using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.AuditLogs.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.AuditLogs.ExportAuditLogs;

public sealed class ExportAuditLogsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AuditLogEndpoints.Export, Handle)
            .WithTags("Audit Logs")
            .WithName("ExportAuditLogs")
            .RequireAuthorization(Policies.AdminOnly);
    }

    private static async Task<IResult> Handle(
        [FromBody] ExportAuditLogsCommand query,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var auditLogsQuery = db.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (query.ActorUserId.HasValue)
        {
            auditLogsQuery = auditLogsQuery
                .Where(log => log.ActorUserId == query.ActorUserId.Value);
        }

        if (query.Action.HasValue)
        {
            auditLogsQuery = auditLogsQuery
                .Where(log => log.Action == query.Action.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            var entityName = query.EntityName.Trim();

            auditLogsQuery = auditLogsQuery
                .Where(log => log.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            var entityId = query.EntityId.Trim();

            auditLogsQuery = auditLogsQuery
                .Where(log => log.EntityId == entityId);
        }

        if (query.From.HasValue)
        {
            auditLogsQuery = auditLogsQuery
                .Where(log => log.CreatedAtUtc >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            auditLogsQuery = auditLogsQuery
                .Where(log => log.CreatedAtUtc <= query.To.Value);
        }

        var items = await auditLogsQuery
            .OrderByDescending(log => log.CreatedAtUtc)
            .Take(10_000)
            .Select(log => new
            {
                log.Id,
                log.ActorUserId,
                Action = log.Action.ToString(),
                log.EntityName,
                log.EntityId,
                log.OldValuesJson,
                log.NewValuesJson,
                log.IpAddress,
                log.UserAgent,
                log.CreatedAtUtc
            })
            .ToListAsync(ct);

        return Results.Ok(items);
    }
}
