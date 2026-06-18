using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Assets.Shared;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.UpdatePost;

public sealed class UpdatePostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut(PostEndpoints.Update, Handle)
            .WithTags("Posts")
            .WithName("UpdatePost")
            .RequireAuthorization(Policies.AuthorOrEditorOrAdmin)
            .AddEndpointFilter<ValidationFilter<UpdatePostCommand>>();
    }

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] UpdatePostCommand request,
        HttpContext httpContext,
        ICurrentUserProvider currentUserProvider,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CacheInvalidationService cacheInvalidationService,
        CancellationToken ct)
    {
        if (currentUserProvider.UserId is null)
        {
            return ResultsFactory.Unauthorized();
        }

        var post = await db.Posts
            .Include(post => post.Categories)
            .Include(post => post.Authors)
            .Include(post => post.Tags)
            .FirstOrDefaultAsync(post => post.Id == id, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(PostErrors.NotFound));
        }

        var isAdminOrEditor =
            httpContext.User.IsInRole(Roles.Admin) ||
            httpContext.User.IsInRole(Roles.Editor);

        var isOwnDraft =
            post.CreatedById == currentUserProvider.UserId.Value &&
            post.IsDraft;

        if (!isAdminOrEditor && !isOwnDraft)
        {
            return Results.Json(
                ApiResponse<object>.Fail(PostErrors.CannotEditPost),
                statusCode: StatusCodes.Status403Forbidden);
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? PostSlugHelper.GenerateSlug(request.Title)
            : PostSlugHelper.GenerateSlug(request.Slug);

        var slugExists = await db.Posts
            .AnyAsync(post => post.Id != id && post.Slug == slug, ct);

        if (slugExists)
        {
            return Results.Conflict(ApiResponse<DomainError>.Fail(PostErrors.SlugAlreadyExists));
        }
        
        if (request.CoverAssetId.HasValue)
        {
            var coverExists = await db.Assets
                .AnyAsync(asset => asset.Id == request.CoverAssetId.Value, ct);

            if (!coverExists)
            {
                return Results.NotFound(ApiResponse<DomainError>.Fail(AssetErrors.NotFound));
            }
        }

        var categoryIds = request.CategoryIds.Distinct().ToArray();
        var authorIds = request.AuthorIds.Distinct().ToArray();

        var existingCategoryCount = await db.Categories
            .CountAsync(category => categoryIds.Contains(category.Id), ct);

        if (existingCategoryCount != categoryIds.Length)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.CategoryNotFound));
        }

        var existingAuthorCount = await db.Authors
            .CountAsync(author => authorIds.Contains(author.Id), ct);

        if (existingAuthorCount != authorIds.Length)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.AuthorNotFound));
        }
        
        var wasFeatured = post.IsFeatured;

        var oldValues = new
        {
            post.Title,
            post.Slug,
            post.Summary,
            post.Content,
            post.CoverAssetId,
            post.Status
        };

        post.Update(
            title: request.Title,
            slug: slug,
            summary: request.Summary,
            content: request.Content,
            coverAssetId: request.CoverAssetId,
            updatedById: currentUserProvider.UserId.Value);

        post.ReplaceCategories(categoryIds);
        post.ReplaceAuthors(authorIds);
        post.ReplaceTags(request.Tags);
        
        auditLogWriter.Add(
            action: AuditAction.PostUpdated,
            entityName: "Post",
            entityId: post.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                post.Title,
                post.Slug,
                post.Summary,
                post.Content,
                post.CoverAssetId,
                post.Status
            });

        await db.SaveChangesAsync(ct);
        if (wasFeatured)
        {
            await cacheInvalidationService.InvalidateFeaturedPostsAsync(ct);
        }

        var response = new UpdatePostResponse
        {
            Post = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Summary = post.Summary,
                Content = post.Content,
                Status = Enum.Parse<PostStatusDto>(post.Status.ToString()),
                IsFeatured = post.IsFeatured,
                FeaturedAtUtc = post.FeaturedAtUtc,
                PublishedAtUtc = post.PublishedAtUtc,
                ScheduledForUtc = post.ScheduledForUtc,
                CoverAssetId = post.CoverAssetId,
                CreatedById = post.CreatedById,
                UpdatedById = post.UpdatedById,
                Tags = post.Tags.Select(tag => tag.Name).ToArray(),
                CreatedAtUtc = post.CreatedAtUtc,
                UpdatedAtUtc = post.UpdatedAtUtc
            }
        };

        return Results.Ok(ApiResponse<UpdatePostResponse>.Ok(response));
    }
}
