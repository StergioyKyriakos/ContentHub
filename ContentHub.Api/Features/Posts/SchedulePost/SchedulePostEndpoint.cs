using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
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
        [FromBody] SchedulePostCommand command,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(PostErrors.NotFound));
        }

        post.Schedule(command.ScheduledForUtc);

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