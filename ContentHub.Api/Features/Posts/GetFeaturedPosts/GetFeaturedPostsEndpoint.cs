using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Data.Entities.Posts; // Ensure your concrete Post entity namespace is imported
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [AsParameters] GetFeaturedPostsQuery query,
        IValidator<GetFeaturedPostsQuery> validator,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var baseQuery = db.Posts
            .AsNoTracking()
            .Where(post => post.Status == PostStatus.Published && post.IsFeatured);

        var totalItems = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(post => post.FeaturedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
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
                query.Page,
                query.PageSize,
                totalItems)
        };

        return Results.Ok(ApiResponse<GetFeaturedPostsResponse>.Ok(response));
    }
}
