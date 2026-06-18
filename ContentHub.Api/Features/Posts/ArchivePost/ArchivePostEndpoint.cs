using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Caching;
using FluentValidation;
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

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] ArchivePostCommand command,
        IValidator<ArchivePostCommand> validator,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CacheInvalidationService cacheInvalidationService,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        if (id != command.Id)
        {
            return ResultsFactory.BadRequest(
                "request.route_body_mismatch",
                "Route id and body id must match.");
        }

        var post = await db.Posts.FirstOrDefaultAsync(post => post.Id == command.Id, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(PostErrors.NotFound));
        }

        var oldValues = new
        {
            post.Id,
            post.Status,
            post.PublishedAtUtc,
            post.IsFeatured,
            post.FeaturedAtUtc
        };

        post.Archive();

        auditLogWriter.Add(
            action: AuditAction.PostArchived,
            entityName: "Post",
            entityId: post.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                post.Id,
                post.Status,
                post.PublishedAtUtc,
                post.IsFeatured,
                post.FeaturedAtUtc
            });

        await db.SaveChangesAsync(ct);
        await cacheInvalidationService.InvalidateFeaturedPostsAsync(ct);
        
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
