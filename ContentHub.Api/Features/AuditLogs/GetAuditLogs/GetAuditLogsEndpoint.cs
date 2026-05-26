using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.AuditLogs.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.AuditLogs;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.AuditLogs.GetAuditLogs;

public sealed class GetAuditLogsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AuditLogEndpoints.GetAll, Handle)
            .WithTags("Audit Logs")
            .WithName("GetAuditLogs")
            .RequireAuthorization(Policies.AdminOnly);
    }

    private static async Task<IResult> Handle(
        GetAuditLogsQuery query,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var currentPage = Math.Max(query.Page, 1);
        var currentPageSize = Math.Clamp(query.PageSize, 1, 100);

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
            var requestedEntityName = query.EntityName.Trim();

            auditLogsQuery = auditLogsQuery
                .Where(log => log.EntityName == requestedEntityName);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            var requestedEntityId = query.EntityId.Trim();

            auditLogsQuery = auditLogsQuery
                .Where(log => log.EntityId == requestedEntityId);
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

        var totalItems = await auditLogsQuery.CountAsync(ct);

        var items = await auditLogsQuery
            .OrderByDescending(log => log.CreatedAtUtc)
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(log => new AuditLogDto
            {
                Id = log.Id,
                ActorUserId = log.ActorUserId,
                Action = log.Action,
                ActionName = log.Action.ToString(),
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                OldValuesJson = log.OldValuesJson,
                NewValuesJson = log.NewValuesJson,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAtUtc = log.CreatedAtUtc
            })
            .ToListAsync(ct);

        var response = new GetAuditLogsResponse
        {
            AuditLogs = PagedResponse<AuditLogDto>.Create(
                items,
                currentPage,
                currentPageSize,
                totalItems)
        };

        return Results.Ok(ApiResponse<GetAuditLogsResponse>.Ok(response));
    }
}
