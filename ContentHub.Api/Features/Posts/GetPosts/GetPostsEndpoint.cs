using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.GetPosts;

public sealed class GetPostsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(PostEndpoints.GetAll, Handle)
            .WithTags("Posts")
            .WithName("GetPosts")
            .RequireAuthorization(Policies.AuthorOrEditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetPostsQuery query,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var postsQuery = db.Posts
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();

            postsQuery = postsQuery.Where(post =>
                post.Title.ToLower().Contains(search) ||
                post.Slug.ToLower().Contains(search));
        }

        if (query.Status.HasValue)
        {
            var status = Enum.Parse<PostStatus>(query.Status.Value.ToString());
            postsQuery = postsQuery.Where(post => post.Status == status);
        }

        if (query.IsFeatured.HasValue)
        {
            postsQuery = postsQuery.Where(post => post.IsFeatured == query.IsFeatured.Value);
        }

        if (query.CategoryId.HasValue)
        {
            postsQuery = postsQuery.Where(post =>
                post.Categories.Any(category => category.CategoryId == query.CategoryId.Value));
        }

        if (query.AuthorId.HasValue)
        {
            postsQuery = postsQuery.Where(post =>
                post.Authors.Any(author => author.AuthorId == query.AuthorId.Value));
        }

        var totalItems = await postsQuery.CountAsync(ct);

        var items = await postsQuery
            .OrderByDescending(post => post.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(post => new PostListItemDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Summary = post.Summary,
                Status = Enum.Parse<PostStatusDto>(post.Status.ToString()),
                IsFeatured = post.IsFeatured,
                PublishedAtUtc = post.PublishedAtUtc,
                ScheduledForUtc = post.ScheduledForUtc,
                CoverAssetId = post.CoverAssetId,
                CreatedAtUtc = post.CreatedAtUtc,
                UpdatedAtUtc = post.UpdatedAtUtc
            })
            .ToListAsync(ct);

        var response = new GetPostsResponse
        {
            Posts = PagedResponse<PostListItemDto>.Create(
                items,
                page,
                pageSize,
                totalItems)
        };

        return Results.Ok(ApiResponse<GetPostsResponse>.Ok(response));
    }
}
