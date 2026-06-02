using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.AuditLogs.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Entities.AuditLogs;
using ContentHub.Data.Dtos.AuditLogs;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.AuditLogs.GetAuditLogById;

public sealed class GetAuditLogByIdEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AuditLogEndpoints.GetById, Handle)
            .WithTags("Audit Logs")
            .WithName("GetAuditLogById")
            .RequireAuthorization(Policies.AdminOnly);
    }

    private static async Task<IResult> Handle(
        [FromBody] GetAuditLogByIdQuery query,
        IValidator<GetAuditLogByIdQuery> validator,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        IQueryable<AuditLog> dbQuery = db.AuditLogs;

        var auditLog = await dbQuery
            .AsNoTracking()
            .Include(log => log.Changes)
            .FirstOrDefaultAsync(log => log.Id == query.Id, ct);

        if (auditLog is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AuditLogErrors.NotFound));
        }

        var response = new GetAuditLogByIdResponse
        {
            AuditLog = new AuditLogDto
            {
                Id = auditLog.Id,
                ActorUserId = auditLog.ActorUserId,
                Action = auditLog.Action,
                ActionName = auditLog.Action.ToString(),
                EntityName = auditLog.EntityName,
                EntityId = auditLog.EntityId,
                OldValuesJson = auditLog.OldValuesJson,
                NewValuesJson = auditLog.NewValuesJson,
                IpAddress = auditLog.IpAddress,
                UserAgent = auditLog.UserAgent,
                CreatedAtUtc = auditLog.CreatedAtUtc,
                Changes = auditLog.Changes
                    .Select(change => new AuditEntityChangeDto
                    {
                        Id = change.Id,
                        PropertyName = change.PropertyName,
                        OldValue = change.OldValue,
                        NewValue = change.NewValue
                    })
                    .ToList()
            }
        };

        return Results.Ok(ApiResponse<GetAuditLogByIdResponse>.Ok(response));
    }
}
