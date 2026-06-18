using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.SchedulePost;

public sealed class SchedulePostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(PostEndpoints.Schedule, Handle)
            .WithTags("Posts")
            .WithName("SchedulePost")
            .RequireAuthorization(Policies.EditorOrAdmin)
            .AddEndpointFilter<ValidationFilter<SchedulePostCommand>>();
    }

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] SchedulePostCommand command,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CancellationToken ct)
    {
        if (id != command.Id)
        {
            return ResultsFactory.BadRequest(
                "request.route_body_mismatch",
                "Route id and body id must match.");
        }

        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(PostErrors.NotFound));
        }

        var oldValues = new
        {
            post.Id,
            post.Status,
            post.ScheduledForUtc
        };

        post.Schedule(command.ScheduledForUtc);

        auditLogWriter.Add(
            action: AuditAction.PostScheduled,
            entityName: "Post",
            entityId: post.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                post.Id,
                post.Status,
                post.ScheduledForUtc
            });

        await db.SaveChangesAsync(ct);

        var response = new SchedulePostResponse
        {
            Id = post.Id,
            Status = Enum.TryParse<PostStatusDto>(post.Status.ToString(), out var parsedStatus)
                ? parsedStatus
                : PostStatusDto.Scheduled,
            ScheduledForUtc = command.ScheduledForUtc
        };

        return Results.Ok(ApiResponse<SchedulePostResponse>.Ok(response));
    }
}
