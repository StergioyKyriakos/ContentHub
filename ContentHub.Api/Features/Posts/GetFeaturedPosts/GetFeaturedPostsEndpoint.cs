using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Abstractions.Caching;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Caching;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ContentHub.Api.Features.Posts.GetFeaturedPosts;

public sealed class GetFeaturedPostsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(PostEndpoints.PublicFeaturedPosts, Handle)
            .WithTags("Public Posts")
            .WithName("GetFeaturedPosts")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [FromBody] GetFeaturedPostsQuery query,
        IValidator<GetFeaturedPostsQuery> validator,
        ContentHubDbContext db,
        ICacheService cacheService,
        IOptions<RedisOptions> redisOptions,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var cacheKey = CacheKeys.PublicFeaturedPosts(page, pageSize);
        var cached = await cacheService.GetAsync<GetFeaturedPostsResponse>(cacheKey, ct);

        if (cached is not null)
        {
            return Results.Ok(ApiResponse<GetFeaturedPostsResponse>.Ok(cached));
        }

        var baseQuery = db.Posts
            .AsNoTracking()
            .Where(post => post.Status == PostStatus.Published && post.IsFeatured);

        var totalItems = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(post => post.FeaturedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(post => new PostSummaryDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Summary = post.Summary,
                IsFeatured = post.IsFeatured,
                PublishedAtUtc = post.PublishedAtUtc,
                CoverAssetId = post.CoverAssetId
            })
            .ToListAsync(ct);

        var response = new GetFeaturedPostsResponse
        {
            Posts = PagedResponse<PostSummaryDto>.Create(
                items,
                page,
                pageSize,
                totalItems)
        };

        await cacheService.SetAsync(
            cacheKey,
            response,
            TimeSpan.FromMinutes(redisOptions.Value.FeaturedPostsCacheMinutes),
            ct);

        return Results.Ok(ApiResponse<GetFeaturedPostsResponse>.Ok(response));
    }
}
