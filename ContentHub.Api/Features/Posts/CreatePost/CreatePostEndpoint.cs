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
using ContentHub.Data.Entities.Posts;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.CreatePost;

public sealed class CreatePostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(PostEndpoints.Create, Handle)
            .WithTags("Posts")
            .WithName("CreatePost")
            .RequireAuthorization(Policies.AuthorOrEditorOrAdmin)
            .AddEndpointFilter<ValidationFilter<CreatePostCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] CreatePostCommand request,
        ICurrentUserProvider currentUserProvider,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CancellationToken ct)
    {
        if (currentUserProvider.UserId is null)
        {
            return Results.Unauthorized();
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? PostSlugHelper.GenerateSlug(request.Title)
            : PostSlugHelper.GenerateSlug(request.Slug);

        var slugExists = await db.Posts
            .AnyAsync(post => post.Slug == slug, ct);

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

        if (categoryIds.Length > 0)
        {
            var existingCategoryCount = await db.Categories
                .CountAsync(category => categoryIds.Contains(category.Id), ct);

            if (existingCategoryCount != categoryIds.Length)
            {
                return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.CategoryNotFound));
            }
        }

        if (authorIds.Length > 0)
        {
            var existingAuthorCount = await db.Authors
                .CountAsync(author => authorIds.Contains(author.Id), ct);

            if (existingAuthorCount != authorIds.Length)
            {
                return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.AuthorNotFound));
            }
        }

        var post = new Post(
            title: request.Title,
            slug: slug,
            summary: request.Summary,
            content: request.Content,
            createdById: currentUserProvider.UserId.Value,
            coverAssetId: request.CoverAssetId);

        post.ReplaceCategories(categoryIds);
        post.ReplaceAuthors(authorIds);
        post.ReplaceTags(request.Tags);

        db.Posts.Add(post);
        
        auditLogWriter.Add(
            action: AuditAction.PostCreated,
            entityName: "Post",
            entityId: post.Id.ToString(),
            newValues: new
            {
                post.Id,
                post.Title,
                post.Slug,
                post.Status,
                post.CreatedById
            });

        await db.SaveChangesAsync(ct);

        var response = new CreatePostResponse
        {
            Post = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Summary = post.Summary,
                Content = post.Content,
                Status = PostStatusDto.Draft,
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

        return Results.Created(
            $"/api/posts/{post.Id}",
            ApiResponse<CreatePostResponse>.Ok(response));
    }
}