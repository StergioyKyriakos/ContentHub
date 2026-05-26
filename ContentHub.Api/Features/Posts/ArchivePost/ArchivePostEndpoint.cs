using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.ArchivePost;

public sealed class ArchivePostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(PostEndpoints.Archive, Handle)
            .WithTags("Posts")
            .WithName("ArchivePost")
            .RequireAuthorization(Policies.EditorOrAdmin);
    }

    private static async Task<IResult> Handle([FromBody] ArchivePostCommand command, ContentHubDbContext db, CancellationToken ct)
    {
        var post = await db.Posts.FirstOrDefaultAsync(post => post.Id == command.Id, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(PostErrors.NotFound));
        }

        post.Archive();
        await db.SaveChangesAsync(ct);
        
        var response = new ArchivePostResponse
        {
            Id = post.Id,
            Status = Enum.TryParse<PostStatusDto>(post.Status.ToString(), out var parsedStatus) 
                ? parsedStatus 
                : PostStatusDto.Archived,
            PublishedAtUtc = post.PublishedAtUtc
        };

        return Results.Ok(ApiResponse<ArchivePostResponse>.Ok(response));
    }
}