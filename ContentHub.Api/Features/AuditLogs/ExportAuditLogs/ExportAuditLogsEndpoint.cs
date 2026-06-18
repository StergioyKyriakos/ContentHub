using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.AuditLogs.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Persistence;
using FluentValidation;
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
        [FromBody] ExportAuditLogsCommand command,
        IValidator<ExportAuditLogsCommand> validator,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        var auditLogsQuery = db.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (command.ActorUserId.HasValue)
        {
            auditLogsQuery = auditLogsQuery
                .Where(log => log.ActorUserId == command.ActorUserId.Value);
        }

        if (command.Action.HasValue)
        {
            auditLogsQuery = auditLogsQuery
                .Where(log => log.Action == command.Action.Value);
        }

        if (!string.IsNullOrWhiteSpace(command.EntityName))
        {
            var entityName = command.EntityName.Trim();

            auditLogsQuery = auditLogsQuery
                .Where(log => log.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(command.EntityId))
        {
            var entityId = command.EntityId.Trim();

            auditLogsQuery = auditLogsQuery
                .Where(log => log.EntityId == entityId);
        }

        if (command.From.HasValue)
        {
            auditLogsQuery = auditLogsQuery
                .Where(log => log.CreatedAtUtc >= command.From.Value);
        }

        if (command.To.HasValue)
        {
            auditLogsQuery = auditLogsQuery
                .Where(log => log.CreatedAtUtc <= command.To.Value);
        }

        var items = await auditLogsQuery
            .OrderByDescending(log => log.CreatedAtUtc)
            .Take(10_000)
            .Select(log => new ExportAuditLogItemResponse
            {
                Id = log.Id,
                ActorUserId = log.ActorUserId,
                Action = log.Action.ToString(),
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                OldValuesJson = log.OldValuesJson,
                NewValuesJson = log.NewValuesJson,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAtUtc = log.CreatedAtUtc
            })
            .ToListAsync(ct);

        var response = new ExportAuditLogsResponse
        {
            Items = items
        };

        return ResultsFactory.Ok(response);
    }
}
