using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.DeletePost;

public sealed class DeletePostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete(PostEndpoints.Delete, Handle)
            .WithTags("Posts")
            .WithName("DeletePost")
            .RequireAuthorization(Policies.EditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        [FromBody] DeletePostCommand command,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CancellationToken ct)
    {
        var post = await db.Posts.FirstOrDefaultAsync(post => post.Id == command.Id, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(PostErrors.NotFound));
        }

        var oldValues = new
        {
            post.Id,
            post.Title,
            post.Slug,
            post.Status,
            post.PublishedAtUtc,
            post.IsFeatured
        };

        post.MarkAsDeleted();

        auditLogWriter.Add(
            action: AuditAction.PostDeleted,
            entityName: "Post",
            entityId: post.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                post.Id,
                post.IsDeleted,
                post.DeletedAtUtc
            });

        await db.SaveChangesAsync(ct);

        var response = new DeletePostResponse();
        return Results.Ok(ApiResponse<DeletePostResponse>.Ok(response));
    }
}
